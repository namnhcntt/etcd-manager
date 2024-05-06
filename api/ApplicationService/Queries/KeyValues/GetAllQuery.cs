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

        public class GetAllQueryHandler(
            IEtcdService _etcdService,
            EtcdManagerDataContext _etcdManagerDataContext,
            IUserPrincipalService _userPrincipalService
        ) : IRequestHandler<GetAllQuery, List<KeyValueResult>>
        {
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
