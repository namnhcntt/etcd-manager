using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetAllQuery: IRequest<List<KeyValueResult>>
    {
        public int EtcdConnectionId { get; set; }

        public class GetAllQueryHandler : IRequestHandler<GetAllQuery, List<KeyValueResult>>
        {
            private readonly IEtcdService _etcdService;
            private readonly EtcdManagerDataContext _etcdManagerDataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public GetAllQueryHandler(IEtcdService etcdService, EtcdManagerDataContext etcdManagerDataContext, IUserPrincipalService userPrincipalService)
            {
                _etcdService = etcdService;
                _etcdManagerDataContext = etcdManagerDataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<List<KeyValueResult>> Handle(GetAllQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _etcdManagerDataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetAll(connectionSetting);
                return result.Adapt<List<KeyValueResult>>();
            }
        }

        public class GetAllQueryValidator: AbstractValidator<GetAllQuery>
        {
            public GetAllQueryValidator()
            {
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
