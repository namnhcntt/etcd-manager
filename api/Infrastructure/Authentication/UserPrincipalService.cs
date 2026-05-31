using System.Security.Claims;
using EtcdManager.API.Domain.Services;

namespace EtcdManager.API.Infrastructure.Authentication;

public class UserPrincipalService(IHttpContextAccessor _httpContextAccessor) : IUserPrincipalService
{
    public int Id
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (idValue != null && int.TryParse(idValue, out var id))
                {
                    return id;
                }
            }
            return -1;
        }
    }

    public string Name
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
