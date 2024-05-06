using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using AutoMapper;
using FluentValidation;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Implement
{
	public class OrderService : IOrderService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		#region Validator
		private readonly CreateOrderModelValidator _createOrderValidator;
		private readonly UpdateOrderModelValidator _updateOrderValidator;
		private readonly DeleteOrderModelValidator _deleteOrderValidator;
		private readonly ChangeOderStatusModelValidator _changeStatusOrderValidator;
		private readonly ChangeOderStatusDeliveredModelValidator _changeStatusOrderDeliveredValidator;
		#endregion
		public OrderService(IUnitOfWork unitOfWork , IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;

			_createOrderValidator = new CreateOrderModelValidator();
			_updateOrderValidator = new UpdateOrderModelValidator();
			_deleteOrderValidator = new DeleteOrderModelValidator();
			_changeStatusOrderValidator = new ChangeOderStatusModelValidator();
			_changeStatusOrderDeliveredValidator = new ChangeOderStatusDeliveredModelValidator();
		}
		#region Get
		public async Task<ResponseObject<OrderModelResponse>> AllOrders()
		{
			var result = new ResponseObject<OrderModelResponse>();
			var orders = await _unitOfWork.OrderRepository.GetAllAsync();
			if ( orders != null && orders.Count > 0 )
			{
				result.StatusCode = 200;
				result.Message = "Order List";
				result.List = _mapper.Map<List<OrderModelResponse>>(orders);
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "Don't have Order!";
				return result;
			}
		}
		public async Task<ResponseObject<OrderModelResponse?>> GetOrderById(string id)
		{
			var result = new ResponseObject<OrderModelResponse?>();
			var orderExist = await _unitOfWork.OrderRepository.GetAsync(id);
			if ( orderExist != null )
			{
				result.StatusCode = 200;
				result.Message = "Order: ";
				result.Data = _mapper.Map<OrderModelResponse>(orderExist);
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "Order not found!";
				return result;
			}
		}
		#endregion
		#region Create Order
		public async Task<ResponseObject<BaseOrderModelResponse>> CreateOrderAsync(CreateOrderModelRequest model)
		{
			var result = new ResponseObject<BaseOrderModelResponse>();
			var validationResult = _createOrderValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check userId exists
			var userExist = await _unitOfWork.UserRepository.GetAsync(model.UserId);
			if ( userExist == null )
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
			var newOrder = _mapper.Map<Order>(model);
			newOrder.User = userExist;
			newOrder.TotalPrice = model.TotalPrice * 1000;
			newOrder.OrderDate = DateTime.Now;
			newOrder.Status = OrderStatus.Processing;
			var createResult = await _unitOfWork.OrderRepository.CreateAsync(newOrder);
			if ( createResult )
			{
				userExist.Orders.Add(newOrder);
				await _unitOfWork.CompleteAsync();
				result.StatusCode = 200;
				result.Message = "Created order of user(" + userExist.FirstName + userExist.LastName + ") successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Create order unsuccessfully!";
				return result;
			}
		}
		#endregion
		public async Task<ResponseObject<BaseOrderModelResponse>> UpdateOrderAsync(UpdateOrderModelRequest model)
		{
			var result = new ResponseObject<BaseOrderModelResponse>();
			var validationResult = _updateOrderValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check order exists
			var orderExist = await _unitOfWork.OrderRepository.GetAsync(model.Id.ToString());
			if ( orderExist == null )
			{
				result.StatusCode = 400;
				result.Message = "Order not found!";
				return result;
			}

			if ( !string.IsNullOrEmpty(model.Note) )
				orderExist.Note = model.Note;
			if ( model.TotalPrice > 0 )
				orderExist.TotalPrice = (double)(model.TotalPrice * 1000);
			if ( model.ShipDate.HasValue )
				orderExist.ShipDate = model.ShipDate.Value;

			var updateResult = await _unitOfWork.OrderRepository.UpdateAsync(orderExist);
			if ( updateResult )
			{
				await _unitOfWork.CompleteAsync();
				result.StatusCode = 200;
				result.Message = "Updated order of user(" + orderExist.User.FirstName + orderExist.User.LastName + ") successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Update order unsuccessfully!";
				return result;
			}
		}
		public async Task<ResponseObject<BaseOrderModelResponse>> DeleteOrderAsync(DeleteOrderModelRequest model)
		{
			var result = new ResponseObject<BaseOrderModelResponse>();
			var validationResult = _deleteOrderValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check order exists
			var orderExist = await _unitOfWork.OrderRepository.GetAsync(model.Id.ToString());
			if ( orderExist == null )
			{
				result.StatusCode = 404;
				result.Message = "Order not found!";
				return result;
			}
			//if user exist in order trust change order status
			var userExist = await _unitOfWork.UserRepository.GetOrderExistInUserAsync(orderExist.UserId.ToString());
			if ( userExist )
			{
				orderExist.Status = OrderStatus.Canceled;
				var updateResult = await _unitOfWork.OrderRepository.UpdateAsync(orderExist);
				if ( updateResult )
				{
					await _unitOfWork.CompleteAsync();
					result.StatusCode = 200;
					result.Message = "Trust change order status into 'canceled' successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					result.Message = "Change order status unsuccessfully!";
					return result;
				}
			}
			var deleteResult = await _unitOfWork.OrderRepository.DeleteAsync(orderExist.Id.ToString());
			if ( deleteResult )
			{
				await _unitOfWork.CompleteAsync();
				result.StatusCode = 200;
				result.Message = "Delete order successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Delete order unsuccessfully!";
				return result;
			}
		}
		#region Change Status Order
		public async Task<ResponseObject<BaseOrderModelResponse>> ChangeOrderStatusAsync(ChangeOderStatusModelRequest model)
		{
			var result = new ResponseObject<BaseOrderModelResponse>();
			var validationResult = _changeStatusOrderValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check order exist
			var orderExist = await _unitOfWork.OrderRepository.GetAsync(model.Id.ToString());
			if ( orderExist == null )
			{
				result.StatusCode = 404;
				result.Message = "Order not found!";
				return result;
			}
			//check order delivered -> if true can't change order status
			if(orderExist.Status == OrderStatus.Delivered )
			{
				result.StatusCode = 400;
				result.Message = "Order has been received! Can't change order status";
				return result;
			}
			var changeResult = await _unitOfWork.OrderRepository.ChangeStatusAsync(orderExist.Id , model.Status);
			if ( changeResult )
			{
				await _unitOfWork.CompleteAsync();
				result.StatusCode = 200;
				result.Message = "Changed order status (" + model.Status.ToString() + ") successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Change order status unsuccessfully!";
				return result;
			}
		}
		public async Task<ResponseObject<OrderModelResponse>> ChangeOrderStatusDeliveredAsync(ChangeOrderStatusDeliveredByUser model)
		{
			var result = new ResponseObject<OrderModelResponse>();
			var validationResult = _changeStatusOrderDeliveredValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			try
			{
				var orderExist = await _unitOfWork.OrderRepository.GetAsync(model.Id.ToString());
				if ( orderExist == null )
				{
					result.StatusCode = 404;
					result.Message = "Order not found!";
					return result;
				}
				//check user to match in order
				if ( orderExist.UserId != model.UserId )
				{
					result.StatusCode = 403;
					result.Message = "Unauthorized to update order status!";
					return result;
				}
				//check transaction exist
				var transactionExist = await _unitOfWork.TransactionRepository.GetTransactionIdStringAsync(model.TransactionId);
				if ( transactionExist != null && transactionExist.Status == TransactionStatus.PAID )
				{
					//check if shipped -> change delivered
					if ( orderExist.Status == OrderStatus.Shipped )
					{
						//update order Status
						orderExist.Status = OrderStatus.Delivered;
						var updateResult = await _unitOfWork.OrderRepository.UpdateAsync(orderExist);
						if ( updateResult )
						{
							await _unitOfWork.CompleteAsync();
							result.StatusCode = 200;
							result.Message = "Order is delivered.";
							return result;
						}
						else
						{
							result.StatusCode = 500;
							result.Message = "Failed to update order status!";
							return result;
						}
					}
					else
					{
						result.StatusCode = 400;
						result.Message = "Order not shipped! Can't check Delivered";
						return result;
					}
				}
				else
				{
					result.StatusCode = 400;
					result.Message = "Unpaid orders!";
					return result;
				}
			}
			catch ( Exception ex )
			{
				result.StatusCode = 500;
				result.Message = "An error occurred while processing the request: " + ex.Message;
				return result;
			}
		}
		#endregion
		public async Task<ResponseObject<OrderModelResponse?>> OrderHistory(string userId)
		{
			var result = new ResponseObject<OrderModelResponse?>();
			var userExist = await _unitOfWork.UserRepository.GetAsync(userId);
			if ( userExist != null )
			{
				var orderHistory = await _unitOfWork.OrderRepository.GetOrderHistoryAsync(userId);
				if ( orderHistory != null && orderHistory.Any() )
				{
					result.StatusCode = 200;
					result.Message = "Order history retrieved successfully";
					result.List = _mapper.Map<List<OrderModelResponse?>>(orderHistory);
				}
				else
				{
					result.StatusCode = 404;
					result.Message = "User does not have any order history";
				}
			}
			else
			{
				result.StatusCode = 400;
				result.Message = $"User with id {userId} not found!";
			}
			return result;
		}

	}
}
