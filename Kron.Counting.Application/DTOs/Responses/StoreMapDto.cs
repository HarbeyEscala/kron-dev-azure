namespace Kron.Counting.Application.DTOs.Responses;

public sealed class StoreMapDto
{
    public Guid StoreId { get; set; }

    public string StoreName { get; set; } = default!;

    public string? Country { get; set; }

    public string? City { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int CurrentOccupancy { get; set; }

    public int OpenAlerts { get; set; }

    public bool IsOnline { get; set; }

    public string Status { get; set; } = default!;
}