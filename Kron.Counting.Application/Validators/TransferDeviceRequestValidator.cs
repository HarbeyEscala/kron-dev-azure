using FluentValidation;
using Kron.Counting.Application.DTOs;

namespace Kron.Counting.Application.Validators;

public class TransferDeviceRequestValidator : AbstractValidator<TransferDeviceRequest>
{
    public TransferDeviceRequestValidator()
    {
        RuleFor(x => x.MeasurementPointId)
            .NotEmpty()
            .WithMessage("MeasurementPointId is required");
    }
}
