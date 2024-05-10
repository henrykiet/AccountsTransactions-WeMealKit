using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Interface
{
	public interface IOrderService
	{
		Task<ResponseObject<OrderModelResponse>> GetAllOrders();
		Task<ResponseObject<OrderModelResponse?>> GetOrderByIdAsync(string id);
		Task<ResponseObject<BaseOrderModelResponse>> CreateOrderAsync(CreateOrderModelRequest model);
		Task<ResponseObject<BaseOrderModelResponse>> UpdateOrderAsync(UpdateOrderModelRequest model);
		Task<ResponseObject<BaseOrderModelResponse>> DeleteOrderAsync(DeleteOrderModelRequest model);
		Task<ResponseObject<BaseOrderModelResponse>> ChangeOrderStatusAsync(ChangeOderStatusModelRequest model);
        Task<ResponseObject<OrderModelResponse?>> OrderHistory(string userId);
		Task<ResponseObject<OrderModelResponse>> ChangeOrderStatusDeliveredAsync(ChangeOrderStatusDeliveredByUser model);
	}
}
