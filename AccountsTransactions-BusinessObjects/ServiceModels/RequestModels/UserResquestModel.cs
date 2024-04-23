using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.RequestModels
{
	public class CreateUserModelRequest
	{
		public string Email { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
        public UserGender Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
		public string? Phone { get; set; }
		public string? Address { get; set; }
		public string Role { get; set; } = string.Empty;
	}
	public class UpdateUserModelRequest
	{
		public string Id { get; set; } = string.Empty;
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public DateTime? DateOfBirth { get; set; }
        public UserGender? Gender { get; set; }
        public string? Phone { get; set; }
		public string? Address { get; set; }
	}
	public class IdUserModelRequest
	{
		public string Id { get; set; } = string.Empty;
	}
	public class ChangeRoleUserModelRequest
	{
		public string Id { get; set; } = string.Empty;
		public string OldRole { get; set; } = string.Empty;
		public string NewRole { get; set; } = string.Empty;
	}
	public class AddRoleUserModelRequest
	{
		public string Id { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}
}
