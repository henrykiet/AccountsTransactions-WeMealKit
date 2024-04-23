using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.RequestModels.Transactions
{
	public class MomoOptionModel
	{
		public string PaymentUrl { get; set; } = string.Empty;
		public string SecretKey { get; set; } = string.Empty;
		public string AccessKey { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
		public string IpnUrl { get; set; } = string.Empty;
		public string PartnerCode { get; set; } = string.Empty;
		public string RequestType { get; set; } = string.Empty;
	}
	public class MomoCreatePaymentModelRequest
	{
		public string partnerCode { get; set; } = string.Empty;
		public string orderId { get; set; } = string.Empty;
		public string requestId { get; set; } = string.Empty;
		public double amount { get; set; }
		public double responseTime { get; set; }
		public string message { get; set; } = string.Empty;
		public int resultCode { get; set; }
		public string payUrl { get; set; } = string.Empty;
		public string shortLink { get; set; } = string.Empty;
	}
	public class OrderInfoModelRequest
	{
		public Guid OrderId { get; set; }
		public string UserId { get; set; } = string.Empty;
	}
	public class CollectionLinkRequest
	{
		public string orderInfo { get; set; } = string.Empty;
		public string partnerCode { get; set; } = string.Empty;
		public string redirectUrl { get; set; } = string.Empty;
		public string ipnUrl { get; set; } = string.Empty;
		public long amount { get; set; }
		public string orderId { get; set; } = string.Empty;
		public string requestId { get; set; } = string.Empty;
		public string extraData { get; set; } = string.Empty;
		public string partnerName { get; set; } = string.Empty;
		public string storeId { get; set; } = string.Empty;
		public string requestType { get; set; } = string.Empty;
		public string orderGroupId { get; set; } = string.Empty;
		public bool autoCapture { get; set; }
		public string lang { get; set; } = string.Empty;
		public string signature { get; set; } = string.Empty;
	}
}
