using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetAllKeysQuery : IRequest<List<string>>
    {
        public int EtcdConnectionId { get; set; }

        public class GetAllKeysQueryHandler(
            IEtcdService _etcdService,
            EtcdManagerDataContext _dataContext,
            IUserPrincipalService _userPrincipalService
            ) : IRequestHandler<GetAllKeysQuery, List<string>>
        {
            public async Task<List<string>> Handle(GetAllKeysQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetAllKeys(connectionSetting);
                return result;
            }
        }
    }
}
