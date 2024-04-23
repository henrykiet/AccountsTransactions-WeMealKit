using AccountAndTransaction_Service;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels.Transactions;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels.Transactions;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using Grpc.Core;

namespace AccountsTransactions_Services.Services
{
	public class TransactionsService : transaction.transactionBase
	{
		private readonly ITransactionService _transactionService;
        public TransactionsService(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
		public override async Task<MomoCreatePaymentResponse> MomoCreatePayment(MomoCreatePaymentRequest request, ServerCallContext context)
		{
			Guid orderId, userId;
			if (Guid.TryParse(request.OrderId, out orderId) && Guid.TryParse(request.UserId, out userId))
			{ 
					var model = new OrderInfoModelRequest
				{
					UserId = userId.ToString(),
					OrderId = orderId,
				};
				var result = await _transactionService.CreatePaymentAsync(model);
				if(result.Data != null)
				{
					return await Task.FromResult(new MomoCreatePaymentResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
						PayUrl = result.Data.payUrl
					});
				}
				else
				{
					return await Task.FromResult(new MomoCreatePaymentResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
					});
				}
			}
			else
			{
				return new MomoCreatePaymentResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<MomoReturnPaymentResponse> MomoReturnPayment(MomoReturnPaymentRequest request , ServerCallContext context)
		{
			if ( request.ResultCode == 0 )
			{
				TransactionStatus transactionStatus = TransactionStatus.UNPAID;
				if ( request.ResultCode == 0 )
				{
					transactionStatus = TransactionStatus.PAID;
				}
				var model = new MomoPaymentReturnModelResponse
				{
					orderId = request.OrderId ?? "" ,
					requestId = request.RequestId ?? "" ,
					orderInfo = request.OrderInfo ?? "" ,
					amount = (long)request.Amount ,
					signature = request.Signature ?? "" ,
					resultCode = transactionStatus ,
					transId = request.TransId ?? "" ,
					extraData = request.ExtraData ?? "" ,
					message = request.Message ?? ""
				};
				var result = await _transactionService.ReturnMomoPaymentAsync(model);
				return await Task.FromResult(new MomoReturnPaymentResponse
				{
					StatusCode = result.StatusCode ,
					Message = result.Message
				});
			}
			else
			{
				return new MomoReturnPaymentResponse
				{
					StatusCode = 400 ,
					Message = "Error in payment momo!"
				};
			}
		}
	}
}
