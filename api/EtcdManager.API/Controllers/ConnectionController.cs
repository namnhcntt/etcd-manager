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

    [HttpGet("CheckConnection")]
    public async Task<IActionResult> CheckConnection(string server, int port, string userName, string password, bool insecure = false)
    {
        return Ok(await this._connectionService.TestConnection(server, port, userName, password, insecure));
    }
}