using EtcdManager.API.Database;
using EtcdManager.API.Models;
using EtcdManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers;

[ApiController]
[Route("[controller]")]

public class AuthenController : ControllerBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILiteDatabaseContext _liteDatabaseContext;

    // constructor
    public AuthenController(
        ILiteDatabaseContext liteDatabaseContext
    )
    {
        this._liteDatabaseContext = liteDatabaseContext;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel loginModel)
    {
        if (loginModel == null)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var loginResult = this._liteDatabaseContext.Login(loginModel.UserName, loginModel.Password);
        return Ok(loginResult);
    }

    [HttpGet("GetUserInfo")]
    public IActionResult GetUserInfo()
    {
        var user = this._liteDatabaseContext.GetUserInfo();
        return Ok(user);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(_liteDatabaseContext.Logout());
    }
}