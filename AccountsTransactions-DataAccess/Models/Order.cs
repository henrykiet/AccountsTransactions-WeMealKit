using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Models
{
	[Table("Orders")]
	public class Order
	{
		[Key]
		public Guid Id { get; set; }
		[ForeignKey(nameof(User))]
		public string UserId { get; set; } = string.Empty;
		public Guid WeekPlanId { get; set; }

		public string? Note { get; set; } = string.Empty;
        public DateTime ShipDate { get; set; }
		public DateTime OrderDate { get; set; }
		public double TotalPrice { get; set; }
		public OrderStatus Status { get; set; }

        //reference
        public User User { get; set; }
		public Transaction Transaction { get; set; } //1 - 1 Transaction with order

		//list
		public List<FeedBack> FeedBacks { get; set; }
    }
}
