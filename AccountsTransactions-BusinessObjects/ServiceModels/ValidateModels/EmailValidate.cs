using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels
{
	public interface IEmailValidator
	{
		bool BeValidEmail(string email);
	}
	public class EmailValidator : IEmailValidator
	{
		public bool BeValidEmail(string email)
		{
			if (string.IsNullOrEmpty(email))
				return true; // Do nothing if email is null or empty

			string emailRegexPattern = @"^[^@\s]+@[^@\d\s]+\.(com|vn)$";

			//check domain
			return Regex.IsMatch(email, emailRegexPattern);
		}
	}
}
