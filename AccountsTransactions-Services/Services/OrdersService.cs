using AccountAndTransaction_Service;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using Grpc.Core;

namespace AccountsTransactions_Services.Services
{
	public class OrdersService : order.orderBase
	{
		private readonly IOrderService _orderService;
        public OrdersService(IOrderService orderService)
        {
            _orderService = orderService;
        }

		public override async Task<AllOrderResponse> AllOrder(AllOrderRequest request, ServerCallContext context)
		{
			var result = await _orderService.AllOrders();
			var orders = new List<Order>();
			if (result.List != null)
			{
				foreach (var order in result.List)
				{
					var o = new Order
					{
						Id = order.Id.ToString(),
						UserId = order.UserId.ToString() ?? "",
						WeekPlanId = order.WeekPlanId.ToString() ?? "",
						Note = order.Note ?? "",
						OrderDate = order.OrderDate.ToString() ?? "",
						TotalPrice = order.TotalPrice,
						ShipDate = order.ShipDate.ToString() ?? "",
						Status = order.Status.ToString() ?? "",
					};
					orders.Add(o);
				}
			}
			return await Task.FromResult(new AllOrderResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message,
				Order = { orders }
			});
		}
		public override async Task<GetOrderByIdResponse> GetOrderById(GetOrderByIdRequest request, ServerCallContext context)
		{
			Guid orderId;
			if (Guid.TryParse(request.Id, out orderId))
			{
				var result = await _orderService.GetOrderById(request.Id);
				if (result.Data != null)
				{
					var o = new Order
					{
						Id = result.Data.Id.ToString(),
						UserId = result.Data.UserId,
						WeekPlanId = result.Data.WeekPlanId.ToString(),
						Note = result.Data.Note ?? "",
						TotalPrice = result.Data.TotalPrice,
						ShipDate = result.Data.ShipDate.ToString(),
						OrderDate = result.Data.OrderDate.ToString(),
						Status = result.Data.Status.ToString(),
					};
					return await Task.FromResult(new GetOrderByIdResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
						Order = o
					});
				}
				return await Task.FromResult(new GetOrderByIdResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new GetOrderByIdResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
		{
			Guid userId, weekPlanId;
			if (Guid.TryParse(request.UserId, out userId) && Guid.TryParse(request.WeekPlanId, out weekPlanId))
			{
				var newOrder = new CreateOrderModelRequest
				{
					UserId = userId.ToString(),
					WeekPlanId = weekPlanId,
					Note = request.Note ?? "",
					TotalPrice = request.TotalPrice,
				// Status = OrderStatusHelper.FromInt(request.Status)
				};
				if (DateTime.TryParse(request.ShipDate, out DateTime shipDate))
				{
					newOrder.ShipDate = shipDate;
				}
				else
				{
					return new BaseOrderResponse
					{
						StatusCode = 400,
						Message = "Invalid Datetime Format!"
					};
				}
				var result = await _orderService.CreateOrderAsync(newOrder);
				return await Task.FromResult(new BaseOrderResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseOrderResponse
				{
					StatusCode = 400,
					Message = "Invalid Id Format!"
				};
			}
		}
		public override async Task<BaseOrderResponse> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
		{
			Guid orderId;
			if (Guid.TryParse(request.Id, out orderId))
			{
				var update = new UpdateOrderModelRequest
				{
					Id = orderId,
					Note = request.Note ?? "",
					TotalPrice = request.TotalPrice,
				};
				if (!string.IsNullOrEmpty(request.ShipDate))
				{
					if (DateTime.TryParse(request.ShipDate, out DateTime shipDate))
					{
						update.ShipDate = shipDate;
					}
					else
					{
						update.ShipDate = null;
					}
				}
				var result = await _orderService.UpdateOrderAsync(update);
				return await Task.FromResult(new BaseOrderResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseOrderResponse
				{
					StatusCode = 400,
					Message = "Invalid Format!"
				};
			}
		}
		public override async Task<BaseOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
		{
			Guid orderId;
			if (Guid.TryParse(request.Id, out orderId))
			{
				var delete = new DeleteOrderModelRequest
				{
					Id = orderId
				};
				var result = await _orderService.DeleteOrderAsync(delete);
				return await Task.FromResult(new BaseOrderResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseOrderResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseOrderResponse> ChangeOrderStatus(ChangOrderStatusRequest request, ServerCallContext context)
		{
			Guid orderId;
			if (Guid.TryParse(request.Id, out orderId))
			{
				var change = new ChangeOderStatusModelRequest
				{
					Id = Guid.Parse(request.Id),
					Status = OrderStatusHelper.FromInt(request.Status)
				};
				var result = await _orderService.ChangeOrderStatusAsync(change);
				return await Task.FromResult(new BaseOrderResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseOrderResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<GetOrderHistoryResponse> GetOrderHistory(GetOrderHistoryRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.UserId, out userId))
			{ 
				var result = await _orderService.OrderHistory(userId.ToString());
				var list = new List<OrderHistory>();
				if (result.List != null)
				{
					foreach (var order in result.List)
					{
						if(order != null)
						{
							var orderHistory = new OrderHistory
							{
								Id = order.Id.ToString(),
								TotalPrice = order.TotalPrice,
								UserId = order.UserId,
								WeekplanId = order.WeekPlanId.ToString(),
								Note = order.Note ?? "",
								OrderDate = order.OrderDate.ToString(),
								ShipDate = order.ShipDate.ToString(),
								Status = order.Status.ToString(),
							};
							list.Add(orderHistory);
						}
					}
					return new GetOrderHistoryResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
						Orders = { list }
					};
				}
				else
				{
					return new GetOrderHistoryResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
					};
				}
			}
			else
			{
				return new GetOrderHistoryResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseOrderResponse> ChangeOrderDeliveredStatus(ChangOrderStatusDeliveredRequest request, ServerCallContext context)
		{
			Guid orderId, userId;
			if (Guid.TryParse(request.Id, out orderId) && Guid.TryParse(request.UserId, out userId))
			{ 
				var model = new ChangeOrderStatusDeliveredByUser
				{
					Id = orderId,
					UserId = userId.ToString(),
					TransactionId = request.TransactionId
				};
				var result = await _orderService.ChangeOrderStatusDeliveredAsync(model);
				return await Task.FromResult(new BaseOrderResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseOrderResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
	}
}
