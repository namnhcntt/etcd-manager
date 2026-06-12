namespace EtcdManager.API.Controllers.Auths.Post;

public class RefreshTokenModel
{
    // optional: browsers carry the refresh token in an HttpOnly cookie instead;
    // the body field remains as a fallback for non-browser clients
    public string? RefreshToken { get; set; }
}
