using AccountsTransactions_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Interface
{
	public interface IUserRepository : IBaseRepository<User>
	{
		Task<bool> GetOrderExistInUserAsync(string idUser);
	}
}
