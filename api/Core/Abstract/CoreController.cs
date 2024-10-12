using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Core.Abstract;

[Route("api/[controller]")]
[ApiController]
public abstract class CoreController : ControllerBase
{
    protected readonly ISender _mediator;

    protected CoreController(ISender mediator)
    {
        _mediator = mediator;
    }
}
