using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels
{
	public class OrderModelResponse
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = string.Empty;
		public Guid WeekPlanId { get; set; }
		public string? Note { get; set; } = string.Empty;
		public DateTime ShipDate { get; set; }
		public DateTime OrderDate { get; set; }
		public double TotalPrice { get; set; }
		public OrderStatus Status { get; set; }
	}
	public class BaseOrderModelResponse
	{
		public Guid Id { get; set; }
		public Guid? UserId { get; set; }
	}
}
