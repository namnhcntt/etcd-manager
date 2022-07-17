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
    public bool CheckConnection(string server, string userName, string password)
    {
        return this._connectionService.TestConnection(server, userName, password);
    }
}