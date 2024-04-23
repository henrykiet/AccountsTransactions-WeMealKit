using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels
{
	public class AllUserResponseModel
	{
		public string ID { get; set; } = string.Empty;
		public string? Email { get; set; } = string.Empty;
		public string? UserName { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime? DateOfBirth { get; set; }
		public string? PhoneNumber { get; set; } = string.Empty;
		public UserGender Gender { get; set; }
		public string Role { get; set; } = string.Empty;
		public string? Address { get; set; } = string.Empty;
		public bool EmailConfirmed { get; set; }
		public DateTimeOffset? LockoutEnd { get; set; }
		public int AccessFailedCount { get; set; }
	}
	public class UserModelResponse
	{
		public Guid ID { get; set; }
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime? DateOfBirth { get; set; }
		public UserGender Gender { get; set; }
		public string PhoneNumber { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}
	public class BaseUserModelResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
	}
}
