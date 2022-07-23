using EtcdManager.API.Database;
using EtcdManager.API.Models;
using EtcdManager.API.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILiteDatabaseContext _liteDatabaseContext;

    // constructor
    public ConnectionController(
        IConnectionService connectionService,
        ILiteDatabaseContext liteDatabaseContext
    )
    {
        this._connectionService = connectionService;
        this._liteDatabaseContext = liteDatabaseContext;
    }

    [HttpPost("CheckConnection")]
    public async Task<IActionResult> CheckConnection([FromBody] ConnectionModel connectionModel)
    {
        return Ok(await this._connectionService.TestConnection(connectionModel));
    }

    [HttpPost]
    public IActionResult CreateConnection([FromBody] ConnectionModel connectionModel)
    {
        return Ok(this._liteDatabaseContext.CreateConnection(connectionModel));
    }

    [HttpPut("{id}")]
    public IActionResult UpdateConnection([FromRoute] Guid id, [FromBody] ConnectionModel connectionModel)
    {
        return Ok(this._liteDatabaseContext.UpdateConnection(id, connectionModel));
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteConnection([FromRoute] Guid id)
    {
        return Ok(this._liteDatabaseContext.DeleteConnection(id));
    }

    [HttpDelete("DeleteConnectionByName")]
    public IActionResult DeleteConnectionByName([FromQuery] string name)
    {
        return Ok(this._liteDatabaseContext.DeleteConnectionByName(name));
    }

    [HttpGet]
    public IActionResult GetConnections()
    {
        return Ok(this._liteDatabaseContext.GetConnections());
    }

    [HttpGet("GetByName")]
    public IActionResult GetConnectionByName([FromQuery] string name)
    {
        return Ok(this._liteDatabaseContext.GetConnectionByName(name));
    }
}