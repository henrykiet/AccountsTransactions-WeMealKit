using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels
{
	public class LoginModelValidator : AbstractValidator<LoginModel>
	{
		private readonly IEmailValidator _emailValidator;
		public LoginModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required!");//check email or username
				//.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
				//.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain!");
			RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required!")
				.Matches(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()])(?=.{6,})")
				.WithMessage("Password must contain at least 1 uppercase letter, 1 digit, 1 special character, and be at least 6 characters long!");
		}
	}
	public class RegisterModelValidator : AbstractValidator<RegisterModel>
	{
		private readonly IEmailValidator _emailValidator;
		public RegisterModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required!")
				.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
				.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain!");
			RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is required!");
			RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is required!");
			// RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required!");
			RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required!")
				.Matches(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()])(?=.{6,})")
				.WithMessage("Password must contain at least 1 uppercase letter, 1 digit, 1 special character, and be at least 6 characters long!");
			RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("ConfirmPassword is required!");
			RuleFor(x => x.Dob)
			.Must(dob =>
			{
				if (dob == null)
					return true;
				var today = DateTime.Today;
				var minDate = today.AddYears(-6);
				return dob < minDate;
			}).WithMessage("Date of birth must be before 6 years!");
		}
	}
	public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordModelRequest>
	{
		public ResetPasswordModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required!");
			RuleFor(x => x.OldPassword).NotEmpty().WithMessage("OldPassword is required!");
			RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("ConfirmPassword is required!");
			RuleFor(x => x.NewPassword).NotEmpty().WithMessage("NewPassword is required!")
							.Matches(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()])(?=.{6,})")
							.WithMessage("Password must contain at least 1 uppercase letter, 1 digit, 1 special character, and be at least 6 characters long!");
		}
	}
	public class ResetPasswordByEmailModelValidator : AbstractValidator<ResetPasswordByEmailModelRequest>
	{
		private readonly IEmailValidator _emailValidator;
		public ResetPasswordByEmailModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required!")
				.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
				.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain!");
			RuleFor(x => x.CodeReset).NotEmpty().WithMessage("Code is required!");
			RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("OldPassword is required!");
			RuleFor(x => x.NewPassword).NotEmpty().WithMessage("NewPassword is required!")
							.Matches(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()])(?=.{6,})")
							.WithMessage("Password must contain at least 1 uppercase letter, 1 digit, 1 special character, and be at least 6 characters long!");
		}
	}
	public class SendCodeByEmailModelValidator : AbstractValidator<SendCodeByEmailModelRequest>
	{
		private readonly IEmailValidator _emailValidator;
		public SendCodeByEmailModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required!")
				.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
				.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain!");
		}
	}
}
