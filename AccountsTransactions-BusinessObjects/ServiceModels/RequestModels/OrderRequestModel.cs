using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.RequestModels
{
	public class CreateOrderModelRequest
	{
		public string UserId { get; set; } = string.Empty;
		public Guid WeekPlanId { get; set; }
		public string? Note { get; set; } = string.Empty;
		public DateTime ShipDate { get; set; }
		public double TotalPrice { get; set; }
	}
	public class UpdateOrderModelRequest
	{
		public Guid Id { get; set; }
		public string? Note { get; set; } = string.Empty;
		public DateTime? ShipDate { get; set; }
		public double? TotalPrice { get; set; }
	}
	public class DeleteOrderModelRequest
	{
		public Guid Id { get; set; }
	}
	public class ChangeOderStatusModelRequest
	{
		public Guid Id { get; set; }
		public OrderStatus Status { get; set; }
	}
	public class ChangeOrderStatusDeliveredByUser
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = string.Empty;
		public string TransactionId { get; set; } = string.Empty;
	}
}
