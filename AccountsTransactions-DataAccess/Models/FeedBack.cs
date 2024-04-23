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
	[Table("FeedBacks")]
	public class FeedBack
	{
		[Key]
		public Guid Id { get; set; }
		[ForeignKey(nameof(Order))]
		public Guid OrderId { get; set; }

		public Rating Rating { get; set; }
		public string? Description { get; set; }
		public IsOrder IsOrder { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public DateTime? UpdatedAt { get; set; }
		public string? UpdatedBy { get; set; }

        //reference
        public virtual Order Order { get; set; }
    }
}
