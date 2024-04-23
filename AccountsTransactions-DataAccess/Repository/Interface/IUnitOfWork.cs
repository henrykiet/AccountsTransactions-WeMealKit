using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Interface
{
	public interface IUnitOfWork
	{
		IUserRepository UserRepository { get; }
		IOrderRepository OrderRepository { get; }
		ITransactionRepository TransactionRepository { get; }
		Task CompleteAsync();
	}
}
