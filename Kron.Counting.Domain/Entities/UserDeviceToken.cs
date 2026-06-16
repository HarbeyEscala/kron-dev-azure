namespace Kron.Counting.Domain.Entities;

public sealed class UserDeviceToken
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = default!;

    public string Platform { get; set; } = default!;

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastUsedAtUtc { get; set; }

    public User? User { get; set; }
}