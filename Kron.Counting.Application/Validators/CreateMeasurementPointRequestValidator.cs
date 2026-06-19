using FluentValidation;
using Kron.Counting.Application.DTOs;

namespace Kron.Counting.Application.Validators;

public class CreateMeasurementPointRequestValidator
    : AbstractValidator<CreateMeasurementPointRequest>
{
    public CreateMeasurementPointRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("StoreId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}
