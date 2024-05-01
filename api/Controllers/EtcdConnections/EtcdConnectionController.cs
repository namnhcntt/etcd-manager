using EtcdManager.API.ApplicationService.Commands.EtcdConnections;
using EtcdManager.API.ApplicationService.Queries.EtcdConnections;
using EtcdManager.API.Controllers.EtcdConnections.Post;
using EtcdManager.API.Controllers.EtcdConnections.Put;
using EtcdManager.API.Core.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Controllers.EtcdConnections
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtcdConnectionController : CoreController
    {
        public EtcdConnectionController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost("TestConnection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionModel model)
        {
            var command = model.Adapt<TestConnectionCommand>();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConnection([FromBody] CreateConnectionModel model)
        {
            var command = model.Adapt<CreateConnectionCommand>();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConnection([FromRoute] int id, [FromBody] UpdateConnectionModel model)
        {
            var command = model.Adapt<UpdateConnectionCommand>();
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConnection([FromRoute] int id)
        {
            var command = new DeleteConnectionCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetConnections()
        {
            var query = new GetConnectionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConnection([FromRoute] int id)
        {
            var query = new GetConnectionByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}
