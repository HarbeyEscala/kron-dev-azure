namespace Kron.Counting.Domain.Constants;

public static class PayloadStatuses
{
    public const string Received = "Received";

    public const string Parsed = "Parsed";

    public const string Saved = "Saved";

    public const string Failed = "Failed";

    public const string Duplicate = "Duplicate";

    public const string DeadLetter = "DeadLetter";

    public const string Acknowledged = "Acknowledged";
}