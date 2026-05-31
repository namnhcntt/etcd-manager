using EtcdManager.API.ApplicationService.Commands.Auths;
using EtcdManager.API.Controllers.Auths.Post;
using EtcdManager.API.Core.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EtcdManager.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator, ILogger<AuthController> logger) : CoreController(mediator)
{
    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var command = model.Adapt<LoginCommand>();
            var token = await _mediator.Send(command);
            return Ok(token);
        }
        catch (Exception)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", model.Username);
            throw;
        }
    }

    [HttpPost("Token/Refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
    {
        var command = model.Adapt<RefreshTokenCommand>();
        var token = await _mediator.Send(command);
        return Ok(token);
    }
}
