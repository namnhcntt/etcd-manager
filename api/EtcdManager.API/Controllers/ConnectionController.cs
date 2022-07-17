using EtcdManager.API.Models;
using EtcdManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly IConnectionService _connectionService;

    // constructor
    public ConnectionController(
        IConnectionService connectionService
    )
    {
        this._connectionService = connectionService;
    }

    [HttpPost("CheckConnection")]
    public async Task<IActionResult> CheckConnection([FromBody] ConnectionModel connectionModel)
    {
        return Ok(await this._connectionService.TestConnection(connectionModel));
    }
}