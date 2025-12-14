using FluentValidation;

namespace MoneyTransfer.Application.Queries.ListTransfers;

public sealed class ListTransfersQueryValidator : AbstractValidator<ListTransfersQuery>
{
    public ListTransfersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage("Status must be a valid transfer status")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.SourceAccount)
            .Must(BeValidIbanFormat)
            .WithMessage("Source account must be a valid IBAN format")
            .When(x => !string.IsNullOrEmpty(x.SourceAccount));

        RuleFor(x => x.DestinationAccount)
            .Must(BeValidIbanFormat)
            .WithMessage("Destination account must be a valid IBAN format")
            .When(x => !string.IsNullOrEmpty(x.DestinationAccount));

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        var validStatuses = new[] { "Pending", "Processing", "Completed", "Failed", "Cancelled" };
        return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidIbanFormat(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return true;

        var normalized = iban.Replace(" ", "").Replace("-", "");
        return normalized.Length >= 15 && normalized.Length <= 34;
    }
}