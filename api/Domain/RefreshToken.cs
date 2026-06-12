namespace EtcdManager.API.Domain;

/// <summary>
/// Persisted record of an issued refresh token (stored as SHA-256 hash).
/// Used for rotation and reuse detection.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
