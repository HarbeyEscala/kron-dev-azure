using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class PatchStoreRequestValidator : AbstractValidator<PatchStoreRequestDto>
{
    public PatchStoreRequestValidator()
    {
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
