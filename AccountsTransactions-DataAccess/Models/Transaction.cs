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
	[Table("Transactions")]
	public class Transaction
	{
		[Key]
		public string Id { get; set; } = string.Empty;
		[ForeignKey(nameof(Order))]
		public Guid OrderId { get; set; }
		//[ForeignKey(nameof(User))]
		//public string UserId { get; set; } = string.Empty;

        public double Amount { get; set; }
		public DateTime TransactionDate { get; set; }
		public string? Notice { get; set; }
		public string? ExtraData { get; set; }
		public string? Signature { get; set; }
		public TransactionStatus Status { get; set; }

		//reference
		//public virtual User User { get; set; }
        public virtual Order Order { get; set; } // 1 - 1 each order trust one transaction

    }
}
