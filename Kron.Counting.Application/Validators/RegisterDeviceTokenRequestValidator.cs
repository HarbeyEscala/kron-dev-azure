using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class RegisterDeviceTokenRequestValidator
    : AbstractValidator<RegisterDeviceTokenRequestDto>
{
    public RegisterDeviceTokenRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage("Platform is required");
    }
}
