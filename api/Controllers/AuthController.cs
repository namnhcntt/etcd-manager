using EtcdManager.API.ApplicationService.Commands.Auth;
using EtcdManager.API.Controllers.Post;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
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
