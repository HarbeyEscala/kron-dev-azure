using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class ConfigureDeviceServerRequestValidator
    : AbstractValidator<ConfigureDeviceServerRequestDto>
{
    public ConfigureDeviceServerRequestValidator()
    {
        RuleFor(x => x.DeviceIp)
            .NotEmpty()
            .WithMessage("DeviceIp is required");

        RuleFor(x => x.ServerUrl)
            .NotEmpty()
            .WithMessage("ServerUrl is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ServerUrl must be a valid URL");
    }
}
