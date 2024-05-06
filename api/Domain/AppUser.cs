namespace EtcdManager.API.Domain
{
    /// <summary>
    /// Represents a user of the application.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Password { get; set; }
    }
}
