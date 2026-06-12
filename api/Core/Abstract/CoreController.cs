using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtcdManager.API.Core.Abstract;

[Authorize]
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
