namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class StoreComparisonAnalyticsDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int Visitors { get; set; }
}