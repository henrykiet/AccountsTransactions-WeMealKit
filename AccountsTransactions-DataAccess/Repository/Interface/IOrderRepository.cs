using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Interface
{
	public interface IOrderRepository : IBaseRepository<Order>
	{
		Task<bool> ChangeStatusAsync(Guid id, OrderStatus status);
		Task<IEnumerable<Order>?> GetOrderHistoryAsync(string userId);
		Task<bool> GetUserExistInOrderAsync(Guid idOrder, string userId);

	}
}
