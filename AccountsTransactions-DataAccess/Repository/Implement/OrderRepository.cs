using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Implement
{
	public class OrderRepository : BaseRepository<Order>, IOrderRepository
	{
        public OrderRepository(AccountsTransactionsContext context) : base(context)
        {
        }
		public override async Task<Order?> GetAsync(string id)
		{
			try
			{
				Guid guidId;
				if (!Guid.TryParse(id, out guidId))
				{
					return null;
				}
				var entity = await _dbSet
					.Include(o => o.User)
					.Include(o => o.Transaction)
					.FirstOrDefaultAsync(o => o.Id == guidId);
				return entity;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in GetAsync: {ex}");
				return null;
			}
		}
		public async Task<bool> ChangeStatusAsync(Guid id, OrderStatus status)
		{
			var orderExist = await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
			if (orderExist != null)
			{
				orderExist.Status = status;
				_dbSet.Update(orderExist);
				return  true;
			}
			return false;
		}

		public async Task<IEnumerable<Order>?> GetOrderHistoryAsync(string userId)
		{
			var result = await _dbSet
				.Include(x => x.Transaction)
				.Include(x => x.User)
				.Where(t => t.UserId == userId).ToListAsync();
			if (result != null)
			{
				return result;
			}
			return null;
		}

		public async Task<bool> GetUserExistInOrderAsync(Guid idOrder, string userId)
		{
			var userExist = await _dbSet.Include(u => u.User).FirstOrDefaultAsync(o => o.Id == idOrder && o.UserId == userId);
			if (userExist != null && userExist.User != null)
			{
				return true;
			}
			return false;
		}
	}
}
