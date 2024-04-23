using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels
{
	public class CreateOrderModelValidator : AbstractValidator<CreateOrderModelRequest>
	{
		public CreateOrderModelValidator()
		{
			RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required!");
			RuleFor(x => x.WeekPlanId).NotEmpty().WithMessage("WeekPlanId is required!");
			RuleFor(x => x.TotalPrice).NotEmpty().WithMessage("Price is required!");
			RuleFor(x => x.ShipDate).NotEmpty().WithMessage("ShipDate is required!")
				.Must(shipDate => shipDate > DateTime.Now)
				.WithMessage("ShipDate must be after the current date.");
		}
	}
	public class UpdateOrderModelValidator : AbstractValidator<UpdateOrderModelRequest>
	{
		public UpdateOrderModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required!");
			RuleFor(x => x.ShipDate).NotEmpty().WithMessage("ShipDate is required!")
				.Must(shipDate => shipDate > DateTime.Now)
				.WithMessage("ShipDate must be after the current date.");
			RuleFor(x => x.TotalPrice).GreaterThan(0).WithMessage("TotalPrice must be greater than 0!");
		}
	}
	public class DeleteOrderModelValidator : AbstractValidator<DeleteOrderModelRequest>
	{
		public DeleteOrderModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required!");
		}
	}
	public class ChangeOderStatusModelValidator : AbstractValidator<ChangeOderStatusModelRequest>
	{
		public ChangeOderStatusModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required!");
			RuleFor(x => x.Status).NotNull().WithMessage("Status is required!");
		}
	}
	public class ChangeOderStatusDeliveredModelValidator : AbstractValidator<ChangeOrderStatusDeliveredByUser>
	{
		public ChangeOderStatusDeliveredModelValidator()
		{
			RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required!");
			RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required!");
			RuleFor(x => x.TransactionId).NotEmpty().WithMessage("TransactionId is required!");
		}
	}
}
