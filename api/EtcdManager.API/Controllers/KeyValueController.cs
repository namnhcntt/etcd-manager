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
        public async Task<IActionResult> GetAll(ConnectionModel connection)
        {
            return Ok(await _keyValueService.GetAll(connection));
        }

        [HttpPost("GetByKeyPrefix")]
        public async Task<IActionResult> GetByKeyPrefix([FromBody] ConnectionModel connection, [FromQuery] string keyPrefix)
        {
            return Ok(await _keyValueService.GetByKeyPrefix(connection, keyPrefix));
        }

        [HttpPost("GetAllKeys")]
        public async Task<IActionResult> GetAllKeys([FromBody] ConnectionModel connection)
        {
            return Ok(await this._keyValueService.GetAllKeys(connection));
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

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromBody] ConnectionModel connection, [FromQuery] string key, [FromQuery] bool deleteRecursive = false)
        {
            return Ok(await this._keyValueService.Delete(connection, key, deleteRecursive));
        }

        [HttpPost("RenameKey")]
        public async Task<IActionResult> RenameKey([FromBody] ConnectionModel connection, [FromQuery] string oldKey, [FromQuery] string newKey)
        {
            return Ok(await this._keyValueService.RenameKey(connection, oldKey, newKey));
        }

        [HttpPost("getrevision")]
        public async Task<IActionResult> GetRevision([FromBody] ConnectionModel connection, [FromQuery] string key)
        {
            return Ok(await this._keyValueService.GetRevisionOfKey(connection, key));
        }

        [HttpPost("importnodes")]
        public async Task<IActionResult> ImportNodes([FromBody] ImportNodesModel importNodesModel)
        {
            return Ok(await this._keyValueService.ImportNodes(importNodesModel.Connection, importNodesModel.KeyModels));
        }
    }
}