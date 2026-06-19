using FluentValidation;
using Kron.Counting.Application.DTOs;

namespace Kron.Counting.Application.Validators;

public class CreateDeviceAssignmentValidator : AbstractValidator<CreateDeviceAssignmentRequest>
{
    public CreateDeviceAssignmentValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("DeviceId is required");

        RuleFor(x => x.MeasurementPointId)
            .NotEmpty()
            .WithMessage("MeasurementPointId is required");
    }
}
