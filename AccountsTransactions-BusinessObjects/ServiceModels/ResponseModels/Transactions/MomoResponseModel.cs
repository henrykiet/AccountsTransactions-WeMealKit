using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels.Transactions
{
	public class MomoPaymentReturnModelResponse
	{
		public string partnerCode { get; set; } = string.Empty;
		public string orderId { get; set; } = string.Empty;
		public string requestId { get; set; } = string.Empty;
		public long amount { get; set; }
		public string orderInfo { get; set; } = string.Empty;
		public string orderType { get; set; } = string.Empty;
		public string transId { get; set; } = string.Empty;
		public TransactionStatus resultCode { get; set; }
		public string message { get; set; } = string.Empty;
		public string payType { get; set; } = string.Empty;
		public string responseTime { get; set; } = string.Empty;
		public string extraData { get; set; } = string.Empty;
		public string signature { get; set; } = string.Empty;
	}
	public class MomoBaseReturnModelResponse
	{

	}
}
