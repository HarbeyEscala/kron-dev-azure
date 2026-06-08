namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class TopStoreAnalyticsDto
{
    public Guid StoreId { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public int Visitors { get; set; }
}