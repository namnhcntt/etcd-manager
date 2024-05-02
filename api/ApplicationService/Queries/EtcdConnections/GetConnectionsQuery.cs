using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static EtcdManager.API.ApplicationService.Queries.EtcdConnections.GetConnectionByIdQuery;
using static EtcdManager.API.ApplicationService.Queries.EtcdConnections.GetConnectionsQuery;

namespace EtcdManager.API.ApplicationService.Queries.EtcdConnections
{
    public class GetConnectionsQuery : IRequest<GetConnectionsQueryResult>
    {
        public class GetConnectionsQueryResult
        {
            public List<GetConnectionByIdQueryResult> Connections { get; set; }
            public int TotalCount
            {
                get
                {
                    return Connections.Count;
                }
            }
        }

        public class GetConnectionsQueryHandler : IRequestHandler<GetConnectionsQuery, GetConnectionsQueryResult>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public GetConnectionsQueryHandler(EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<GetConnectionsQueryResult> Handle(GetConnectionsQuery request, CancellationToken cancellationToken)
            {
                var ownerId = _userPrincipalService.Id;
                var connections = await _dataContext.EtcdConnections.Where(x => x.OwnerId == ownerId).ToListAsync();
                return new GetConnectionsQueryResult
                {
                    Connections = connections.Adapt<List<GetConnectionByIdQueryResult>>()
                };
            }
        }


    }
}
