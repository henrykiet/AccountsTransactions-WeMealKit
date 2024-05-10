using AccountsTransactions_BusinessObjects.Helpers;
using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels.Transactions;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels.Transactions;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using Hangfire;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Implement
{
	public class TransactionService : ITransactionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOptions<MomoOptionModel> _momoOptions;

		public TransactionService(IUnitOfWork unitOfWork , IOptions<MomoOptionModel> momoOptions)
		{
			_unitOfWork = unitOfWork;
			_momoOptions = momoOptions;
		}
		#region Momo
		public async Task<ResponseObject<MomoCreatePaymentModelRequest>> CreatePaymentAsync(OrderInfoModelRequest model)
		{
			var result = new ResponseObject<MomoCreatePaymentModelRequest>();
			//check order exist
			var orderExist = await _unitOfWork.OrderRepository.GetByIdAsync(model.OrderId.ToString());
			if ( orderExist == null )
			{
				result.StatusCode = 404;
				result.Message = "Order does not exist!";
				return result;
			}
			var transactionExist = orderExist.Transaction;
			if ( transactionExist != null && transactionExist.Status == TransactionStatus.PAID )
			{
				result.StatusCode = 400;
				result.Message = "Order is payed!";
				return result;
			}
			//check user exist in order
			var userExist = await _unitOfWork.OrderRepository.GetUserExistInOrderAsync(model.OrderId , model.UserId);
			if ( userExist == false )
			{
				result.StatusCode = 400;
				result.Message = "User does not match with order!";
				return result;
			}
			//create new request id
			var requestId = Guid.NewGuid();
			var orderInfo = "Order from: " + orderExist.User.FirstName + orderExist.User.LastName;
			//create model request
			CollectionLinkRequest request = new CollectionLinkRequest();
			request.orderInfo = orderInfo;
			request.partnerCode = _momoOptions.Value.PartnerCode;
			request.ipnUrl = _momoOptions.Value.IpnUrl;
			request.redirectUrl = _momoOptions.Value.ReturnUrl;
			if ( orderExist.TotalPrice <= 0 )
			{
				result.StatusCode = 404;
				result.Message = "Order price is null!";
				return result;
			}
			request.amount = (long)orderExist.TotalPrice;
			request.orderId = orderExist.Id.ToString();
			request.requestId = requestId.ToString();
			request.requestType = _momoOptions.Value.RequestType;
			request.extraData = model.UserId;
			// request.partnerName = "WEMEALKIT";
			// request.storeId = "Test Store";
			// request.orderGroupId = "";
			request.autoCapture = true;
			request.lang = "vi";
			var rawSignature = "accessKey=" + _momoOptions.Value.AccessKey
				+ "&amount=" + request.amount
				+ "&extraData=" + request.extraData
				+ "&ipnUrl=" + request.ipnUrl
				+ "&orderId=" + request.orderId
				+ "&orderInfo=" + request.orderInfo
				+ "&partnerCode=" + request.partnerCode
				+ "&redirectUrl=" + request.redirectUrl
				+ "&requestId=" + request.requestId
				+ "&requestType=" + request.requestType;
			request.signature = HashHelper.GetSignature256(rawSignature , _momoOptions.Value.SecretKey);
			StringContent httpContent = new StringContent(JsonSerializer.Serialize(request) , System.Text.Encoding.UTF8 , "application/json");
			var client = new HttpClient();
			var quickPayResponse = await client.PostAsync(_momoOptions.Value.PaymentUrl , httpContent);
			var contents = quickPayResponse.Content.ReadAsStringAsync().Result;
			var deserialize = JsonSerializer.Deserialize<MomoCreatePaymentModelRequest>(contents);
			if ( deserialize != null )
			{
				result.StatusCode = deserialize.resultCode;
				result.Message = deserialize.message;
				result.Data = JsonSerializer.Deserialize<MomoCreatePaymentModelRequest>(contents);
				return result;
			}
			else
			{
				result.StatusCode = 400;
				result.Message = "Fail call to MoMo!";
				return result;
			}
		}
		public async Task<ResponseObject<MomoBaseReturnModelResponse>> ReturnMomoPaymentAsync(MomoPaymentReturnModelResponse model)
		{
			var result = new ResponseObject<MomoBaseReturnModelResponse>();
			try
			{
				//check order exist
				var orderExist = await _unitOfWork.OrderRepository.GetByIdAsync(model.orderId);
				if ( orderExist != null )
				{
					//check userExist
					var userExist = await _unitOfWork.OrderRepository.GetUserExistInOrderAsync(orderExist.Id , model.extraData);
					if ( userExist && model.resultCode == 0 )
					{
						//create transaction
						var transaction = new Transaction
						{
							Id = model.transId ,
							ExtraData = model.extraData ,
							OrderId = Guid.Parse(model.orderId) ,
							Notice = model.orderInfo ,
							Amount = model.amount ,
							TransactionDate = DateTime.Now ,
							Status = model.resultCode ,
							Signature = model.signature
						};
						//set 1-1
						transaction.Order = orderExist;
						orderExist.Transaction = transaction;
						var newTransaction = await _unitOfWork.TransactionRepository.CreateAsync(transaction);
						if ( newTransaction )
						{
							//Successfully
							//set orderStatus -> shipping
							var orderStatusResult = await _unitOfWork.OrderRepository.ChangeStatusAsync(orderExist.Id , OrderStatus.Shipping);
							if ( orderStatusResult )
							{
								//call hangfire -> make order status to unShiped when cometo shipdate but not shiped 
								var shipDate = Convert.ToDateTime(orderExist.ShipDate);
								TimeSpan jobDelay = shipDate.Subtract(DateTime.Now);
								BackgroundJob.Schedule(() => CheckAndUpdateOrderStatus(orderExist.Id) , jobDelay);
								await _unitOfWork.CompleteAsync();
								result.StatusCode = 200;
								result.Message = "Transaction successfully.";
								return result;
							}
							result.StatusCode = 500;
							result.Message = "Change Order status unsuccessfully!";
							return result;

						}
						else
						{
							//unsuccess
							result.StatusCode = 500;
							result.Message = "Transaction fail!";
							return result;
						}
					}
					else
					{
						//User un exist
						result.StatusCode = 400;
						result.Message = "User doesn't exist!";
						return result;
					}
				}
				else
				{
					result.StatusCode = 400;
					result.Message = "Order not exist!";
					return result;
				}
			}
			catch ( Exception e )
			{
				result.StatusCode = 400;
				result.Message = "Have error when transaction!" + e.Message;
				return result;
			}
		}
		public async Task CheckAndUpdateOrderStatus(Guid orderId)
		{
			var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId.ToString());
			if ( order != null && order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Canceled && order.ShipDate == DateTime.Now )
			{
				//chang order status
				order.Status = OrderStatus.UnShipped;
				await _unitOfWork.CompleteAsync();
			}
		}
		#endregion
	}
}
