using EtcdManager.API.ApplicationService.Commands.Auths;
using EtcdManager.API.Controllers.Auths.Post;
using EtcdManager.API.Core.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : CoreController
    {
        public AuthController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var command = model.Adapt<LoginCommand>();
            var token = await _mediator.Send(command);
            return Ok(token);
        }

        [HttpPost("Token/Refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var command = model.Adapt<RefreshTokenCommand>();
            var token = await _mediator.Send(command);
            return Ok(token);
        }
    }
}
