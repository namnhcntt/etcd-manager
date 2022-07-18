using EtcdManager.API.Models;
using EtcdManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeyValueController : ControllerBase
    {
        private readonly IKeyValueService _keyValueService;

        public KeyValueController(
            IKeyValueService keyValueService
        )
        {
            this._keyValueService = keyValueService;
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> Get([FromBody] ConnectionModel connection)
        {
            return Ok(await this._keyValueService.GetAll(connection));
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] ConnectionModel connection, [FromQuery] string key)
        {
            return Ok(await this._keyValueService.Get(connection, key));
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] SaveKeyModel keyModel)
        {
            return Ok(await this._keyValueService.Save(keyModel));
        }
    }
}