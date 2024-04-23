using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels.Transactions;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Interface
{
	public interface ITransactionService
	{
		Task<ResponseObject<MomoCreatePaymentModelRequest>> CreatePaymentAsync(OrderInfoModelRequest model);
		Task<ResponseObject<MomoBaseReturnModelResponse>> ReturnMomoPaymentAsync(MomoPaymentReturnModelResponse model);

	}
}
