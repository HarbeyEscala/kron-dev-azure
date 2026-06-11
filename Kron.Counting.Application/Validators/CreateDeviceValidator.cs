using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceRequestDto>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .WithMessage("SerialNumber is required")
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.DeviceType)
            .NotEmpty()
            .WithMessage("DeviceType is required")
            .MaximumLength(100);

        RuleFor(x => x.FirmwareVersion)
            .MaximumLength(50);
    }
}