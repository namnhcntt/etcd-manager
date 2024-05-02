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

        public class GetAllKeysQueryHandler : IRequestHandler<GetAllKeysQuery, List<string>>
        {
            private readonly IEtcdService _etcdService;
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public GetAllKeysQueryHandler(IEtcdService etcdService, EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _etcdService = etcdService;
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

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
