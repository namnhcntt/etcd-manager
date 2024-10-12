using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static EtcdManager.API.ApplicationService.Queries.EtcdConnections.GetConnectionByIdQuery;

namespace EtcdManager.API.ApplicationService.Queries.EtcdConnections;

public class GetConnectionByIdQuery : IRequest<GetConnectionByIdQueryResult>
{
    public int Id { get; set; }

    public class GetConnectionByIdQueryResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Server { get; set; } = null!;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? PermissionUsers { get; set; }
        public bool EnableAuthenticated { get; set; }
        public bool Insecure { get; set; }
        public string? AgentDomain { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetConnectionByIdQueryHandler(
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService
    ) : IRequestHandler<GetConnectionByIdQuery, GetConnectionByIdQueryResult>
    {
        public async Task<GetConnectionByIdQueryResult> Handle(
            GetConnectionByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var ownerId = _userPrincipalService.Id;
            var connection = await _dataContext.EtcdConnections.FirstOrDefaultAsync(x =>
                x.Id == request.Id && x.OwnerId == ownerId
            );
            if (connection != null)
            {
                return connection.Adapt<GetConnectionByIdQueryResult>();
            }
            throw new Exception("Connection not found");
        }
    }
}
