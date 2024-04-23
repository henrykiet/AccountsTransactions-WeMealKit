using AccountsTransactions_DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.RequestModels
{
	public class LoginModel
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
	public class RegisterModel
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime? Dob { get; set; }
		public UserGender Gender { get; set; }
		public string? PhoneNumber { get; set; } = string.Empty;
	}
	public class ConfirmMailModel
	{
		public string Email { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
	}
	public class ResetPasswordModelRequest
	{
		public string Id { get; set; } = string.Empty;
		public string OldPassword { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
	}
	public class ResetPasswordByEmailModelRequest
	{
		public string Email { get; set; } = string.Empty;
		public string CodeReset { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
	}
	public class SendCodeByEmailModelRequest
	{
		public string Email { get; set; } = string.Empty;
	}
}
