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
	public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
	{
        public TransactionRepository(AccountsTransactionsContext context) : base(context) 
        {
            
        }

		public async Task<Transaction?> GetTransactionIdStringAsync(string transactionId)
		{
			var result = await _dbSet.FirstOrDefaultAsync(t => t.Id == transactionId);
			if (result != null)
			{
				return result;
			}
			return null;
		}
	}
}
