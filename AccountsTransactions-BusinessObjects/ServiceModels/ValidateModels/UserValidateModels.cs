using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using FluentValidation.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels
{
	public class CreateUserModelValidator : AbstractValidator<CreateUserModelRequest>
	{
		private readonly IEmailValidator _emailValidator;
		public CreateUserModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required!")
				.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
				.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain!");
			RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required!");
			RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is required!");
			RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is required!");
			RuleFor(x => x.DateOfBirth)
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
	public class UpdateModelValidator : AbstractValidator<UpdateUserModelRequest>
	{
		private readonly IEmailValidator _emailValidator;
		public UpdateModelValidator()
		{
			_emailValidator = new EmailValidator();
			RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required!");
			RuleFor(x => x.Email)
			.Empty().When(x => x.Email == null).WithMessage("Email should be null.")
			.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format!")
			.Must(_emailValidator.BeValidEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email domain.");
			RuleFor(x => x.DateOfBirth)
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
	public class IdUserModelValidator : AbstractValidator<IdUserModelRequest>
	{
		public IdUserModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required!");
		}
	}
	public class AddRoleUserModelValidator : AbstractValidator<AddRoleUserModelRequest>
	{
		public AddRoleUserModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required!");
			RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required!");
		}
	}
	public class ChangeRoleUserModelValidator : AbstractValidator<ChangeRoleUserModelRequest>
	{
		public ChangeRoleUserModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required!");
			RuleFor(x => x.OldRole).NotEmpty().WithMessage("OldRole is required!");
			RuleFor(x => x.NewRole).NotEmpty().WithMessage("NewRole is required!");
		}
	}
}
