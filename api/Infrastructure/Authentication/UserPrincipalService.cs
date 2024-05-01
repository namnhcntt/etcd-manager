using EtcdManager.API.Domain.Services;
using System.Security.Claims;

namespace EtcdManager.API.Infrastructure.Authentication
{
    public class UserPrincipalService : IUserPrincipalService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserPrincipalService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int Id {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return int.Parse(id);
                }
                return -1;
            }
        }

        public string Name {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    return user.FindFirst(ClaimTypes.Name)?.Value;
                }
                return string.Empty;
            }
        }
    }
}
