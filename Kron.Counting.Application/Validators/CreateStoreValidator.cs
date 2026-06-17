using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public sealed class CreateStoreValidator
    : AbstractValidator<CreateStoreRequestDto>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.StoreType)
            .MaximumLength(100);

        RuleFor(x => x.Region)
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .MaximumLength(100);

        RuleFor(x => x.State)
            .MaximumLength(100);

        RuleFor(x => x.City)
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .MaximumLength(50);

        RuleFor(x => x.AddressLine1)
            .MaximumLength(250);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(250);

        RuleFor(x => x.TimeZone)
            .MaximumLength(100);

        RuleFor(x => x.ContactName)
            .MaximumLength(100);

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50);

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Capacity.HasValue);
    }
}