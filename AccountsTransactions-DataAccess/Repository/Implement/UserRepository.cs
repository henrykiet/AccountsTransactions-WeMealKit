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
	public class UserRepository : BaseRepository<User>, IUserRepository
	{
		public UserRepository(AccountsTransactionsContext context) : base(context)
		{
		}
		public override Task<List<User>> GetAllAsync()
		{
			return base.GetAllAsync();
		}
		public override async Task<User?> GetAsync(string id)
		{
			return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
		}
		public override Task<bool> CreateAsync(User entity)
		{
			return base.CreateAsync(entity);
		}
		public override async Task<bool> UpdateAsync(User entity)
		{
			try
			{
				var existData = await _dbSet.FirstOrDefaultAsync(u => u.Id.Equals(entity.Id));
				if (existData != null)
				{
					existData.UserName = entity.UserName;
					return true;
				}
				else
				{
					return false;
				}
			}catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in UpdateAsync: {ex}");
				return false;
			}
		}
		public override Task<bool> DeleteAsync(string id)
		{
			return base.DeleteAsync(id);
		}
		public async Task<bool> GetOrderExistInUserAsync(string idUser)
		{
			var userExist = await _dbSet.Include(u => u.Orders).FirstOrDefaultAsync(o => o.Id == idUser);
			if (userExist != null && userExist.Orders != null)
			{
				return true;
			}
			return false;
		}

	}
}
