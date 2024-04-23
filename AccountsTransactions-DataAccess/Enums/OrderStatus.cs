using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Enums
{
	public enum OrderStatus
	{
		Processing = 0,
		Shipping = 1,
		Shipped = 2,
		UnShipped = 3,
		Delivered = 4,
		Canceled = 5
	}
	public static class OrderStatusHelper
	{
		public static int ToInt(this OrderStatus status)
		{
			return (int)status;
		}
		//convert int to OrderStatus
		public static OrderStatus FromInt(int value)
		{
			return Enum.IsDefined(typeof(OrderStatus), value) ? (OrderStatus)value : OrderStatus.Canceled;
		}
	}
}
