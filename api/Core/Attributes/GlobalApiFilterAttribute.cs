using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace EtcdManager.API.Core.Attributes;

public class GlobalApiFilterAttribute : ActionFilterAttribute
{
    public GlobalApiFilterAttribute() { }

    public bool AllowAllAuthenticatedUser { get; set; }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        if (
            context.ActionDescriptor.EndpointMetadata.Any(f =>
                f.GetType() == typeof(AllowAnonymousAttribute)
            )
        )
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        if (user.Identity == null || (user.Identity != null && !user.Identity.IsAuthenticated))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}
