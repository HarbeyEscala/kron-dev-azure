using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class ProvisionDeviceRequestValidator
    : AbstractValidator<ProvisionDeviceRequestDto>
{
    public ProvisionDeviceRequestValidator()
    {
        RuleFor(x => x.MeasurementPointId)
            .NotEmpty()
            .WithMessage("MeasurementPointId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}
