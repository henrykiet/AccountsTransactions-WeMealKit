using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Implement
{
	public class UnitOfWork : IUnitOfWork
	{
		public IUserRepository UserRepository {  get; private set; }
		public IOrderRepository OrderRepository {  get; private set; }
		public ITransactionRepository TransactionRepository { get; private set; }

		private readonly AccountsTransactionsContext _context;
        public UnitOfWork(AccountsTransactionsContext context)
        {
            this._context = context;
			UserRepository = new UserRepository(context);
			OrderRepository = new OrderRepository(context);
			TransactionRepository = new TransactionRepository(context);
		}

        public async Task CompleteAsync()
		{
			await this._context.SaveChangesAsync();
		}
	}
}
