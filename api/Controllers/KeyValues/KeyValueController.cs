using EtcdManager.API.ApplicationService.Queries.KeyValues;
using EtcdManager.API.Controllers.KeyValues.Post;
using EtcdManager.API.Core.Abstract;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers.KeyValues
{
    public class KeyValueController : CoreController
    {
        public KeyValueController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetAllKeysQuery()
            {
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetByKeyPrefix")]
        public async Task<IActionResult> GetByKeyPrefix([FromQuery] string keyPrefix)
        {
            return Ok();
        }

        [HttpGet("GetAllKeys")]
        public async Task<IActionResult> GetAllKeys([FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetAllKeysQuery()
            {
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetByKey")]
        public async Task<IActionResult> GetByKey([FromQuery] string key, [FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetByKeyQuery()
            {
                Key = key,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] SaveKeyValueModel model)
        {
            return Ok();
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete([FromRoute] string key, [FromQuery] bool deleteRecursive = false)
        {
            return Ok();
        }

        [HttpPost("RenameKey")]
        public async Task<IActionResult> RenameKey([FromBody] RenameKeyModel model)
        {
            return Ok();
        }

        [HttpPost("GetRevision/{key}")]
        public async Task<IActionResult> GetRevision([FromRoute] string key)
        {
            return Ok();
        }

        [HttpPost("ImportNodes")]
        public async Task<IActionResult> ImportNodes([FromBody] ImportNodesModel model)
        {
            return Ok();
        }
    }
}
