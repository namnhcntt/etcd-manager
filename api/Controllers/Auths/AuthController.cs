using EtcdManager.API.ApplicationService.Commands.Auths;
using EtcdManager.API.Controllers.Auths.Post;
using EtcdManager.API.Core.Abstract;
using EtcdManager.API.Domain.Services;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace EtcdManager.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator, ITokenService tokenService, ILogger<AuthController> logger) : CoreController(mediator)
{
    // F009: the refresh token is delivered to browsers via an HttpOnly cookie so it is
    // unreachable from JavaScript (XSS). Path-scoped to the auth endpoints only.
    private const string RefreshTokenCookieName = "etcd_manager_refresh_token";
    // lowercase on purpose: RFC 6265 cookie path-matching is case-sensitive in browsers
    // and the UI calls /api/auth/...; ASP.NET Core routing is case-insensitive so the
    // server side is unaffected
    private const string RefreshTokenCookiePath = "/api/auth";

    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var command = model.Adapt<LoginCommand>();
            var token = await _mediator.Send(command);
            SetRefreshTokenCookie(token.RefreshToken, token.RefreshTokenExpiresAt);
            // RefreshToken / RefreshTokenExpiresAt are [JsonIgnore]d — body carries access token + expiry only
            return Ok(token);
        }
        catch (Exception)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", model.Username);
            throw;
        }
    }

    [HttpPost("Token/Refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> RefreshToken(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenModel? model
    )
    {
        // prefer the HttpOnly cookie (browser flow); fall back to the body for non-browser clients
        var refreshToken = Request.Cookies[RefreshTokenCookieName] ?? model?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Missing refresh token" });
        }

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var token = await _mediator.Send(command);
        // rotation: the old token was consumed, set the newly issued one
        SetRefreshTokenCookie(token.RefreshToken, token.RefreshTokenExpiresAt);
        return Ok(token);
    }

    // AllowAnonymous: logout must work even when the access token has already expired;
    // the only thing it acts on is the caller's own refresh cookie, which is revoked server-side.
    [HttpPost("Logout")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await tokenService.RevokeRefreshToken(refreshToken);
        }
        Response.Cookies.Delete(RefreshTokenCookieName, BuildRefreshTokenCookieOptions(null));
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            BuildRefreshTokenCookieOptions(expiresAtUtc)
        );
    }

    private static CookieOptions BuildRefreshTokenCookieOptions(DateTime? expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            // UI and API may be served from different origins (nginx serves only the static
            // bundle; the browser calls the API directly) → SameSite=None + Secure is required
            // for the cookie to be sent cross-origin. CORS uses explicit origins + AllowCredentials.
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = RefreshTokenCookiePath,
            Expires = expiresAtUtc.HasValue ? new DateTimeOffset(expiresAtUtc.Value, TimeSpan.Zero) : null,
        };
    }
}
