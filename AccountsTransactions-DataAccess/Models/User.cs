using System.ComponentModel.DataAnnotations.Schema;
using AccountsTransactions_DataAccess.Enums;
using Microsoft.AspNetCore.Identity;

namespace AccountsTransactions_DataAccess.Models
{
    [Table("Users")]
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserGender Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Code { get; set; }

        //list
		public List<Order> Orders { get; set; }
	}
}