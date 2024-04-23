using AccountsTransactions_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Interface
{
	public interface ITransactionRepository : IBaseRepository<Transaction>
	{
		Task<Transaction?> GetTransactionIdStringAsync(string transactionId);

	}
}
