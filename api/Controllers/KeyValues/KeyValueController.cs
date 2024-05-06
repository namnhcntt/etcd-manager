using EtcdManager.API.ApplicationService.Commands.KeyValues;
using EtcdManager.API.ApplicationService.Queries.KeyValues;
using EtcdManager.API.Controllers.KeyValues.Post;
using EtcdManager.API.Core.Abstract;
using EtcdManager.API.Domain;
using Mapster;
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
            var query = new GetAllQuery()
            {
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
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

        [HttpGet("GetByKeyPrefix")]
        public async Task<IActionResult> GetByKeyPrefix([FromQuery] string keyPrefix, [FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetByKeyPrefixQuery()
            {
                Prefix = keyPrefix,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] SaveKeyValueModel model, [FromQuery] int selectedEtcdConnectionId)
        {
            var command = model.Adapt<SaveCommand>();
            command.EtcdConnectionId = selectedEtcdConnectionId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("DeleteKey")]
        public async Task<IActionResult> Delete([FromQuery] string key, [FromQuery] int selectedEtcdConnectionId, [FromQuery] bool deleteRecursive = false)
        {
            var command = new DeleteKeyCommand()
            {
                Key = key,
                DeleteRecursive = deleteRecursive,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("RenameKey")]
        public async Task<IActionResult> RenameKey([FromBody] RenameKeyModel model, [FromQuery] int selectedEtcdConnectionId)
        {
            var command = model.Adapt<RenameKeyCommand>();
            command.EtcdConnectionId = selectedEtcdConnectionId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetRevision")]
        public async Task<IActionResult> GetRevision([FromQuery] string key, [FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetRevisionQuery()
            {
                Key = key,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetRevisionDetail")]
        public async Task<IActionResult> GetRevisionDetail([FromQuery] string key, [FromQuery] long revision, [FromQuery] int selectedEtcdConnectionId)
        {
            var query = new GetRevisionDetailQuery()
            {
                Key = key,
                Revision = revision,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("ImportNodes")]
        public async Task<IActionResult> ImportNodes([FromBody] KeyValue[] KeyValues, [FromQuery] int selectedEtcdConnectionId)
        {
            var command = new ImportNodesCommand
            {
                KeyValues = KeyValues,
                EtcdConnectionId = selectedEtcdConnectionId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
