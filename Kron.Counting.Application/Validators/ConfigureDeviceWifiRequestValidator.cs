using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class ConfigureDeviceWifiRequestValidator
    : AbstractValidator<ConfigureDeviceWifiRequestDto>
{
    public ConfigureDeviceWifiRequestValidator()
    {
        RuleFor(x => x.DeviceIp)
            .NotEmpty()
            .WithMessage("DeviceIp is required");

        RuleFor(x => x.Ssid)
            .NotEmpty()
            .WithMessage("Ssid is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
