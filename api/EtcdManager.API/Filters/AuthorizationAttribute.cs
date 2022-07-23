using EtcdManager.API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EtcdManager.API.Filters
{
    public class AuthorizationAttribute : ActionFilterAttribute
    {
        public AuthorizationAttribute()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionDescriptor.EndpointMetadata.Any(f => f.GetType() == typeof(AllowAnonymousAttribute)))
            {
                var liteDatabaseContext = context.HttpContext.RequestServices.GetRequiredService<ILiteDatabaseContext>();

                var token = context.HttpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    context.Result = new UnauthorizedResult();
                }
                else if (!liteDatabaseContext.TokenIsValid(token).Data)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}