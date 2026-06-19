using FluentValidation;
using Kron.Counting.Application.DTOs.Requests;

namespace Kron.Counting.Application.Validators;

public class TestNotificationRequestValidator
    : AbstractValidator<TestNotificationRequestDto>
{
    public TestNotificationRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Body is required");
    }
}
