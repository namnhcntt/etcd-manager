using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetByKeyQuery: IRequest<KeyValueResult>
    {
        public string Key { get; set; } = null!;
        public int EtcdConnectionId { get; set; }

        public class GetByKeyQueryHandler(
            IEtcdService _etcdService,
            EtcdManagerDataContext _dataContext,
            IUserPrincipalService _userPrincipalService
            ): IRequestHandler<GetByKeyQuery, KeyValueResult>
        {
            public async Task<KeyValueResult> Handle(GetByKeyQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetByKey(request.Key, connectionSetting);
                return result.Adapt<KeyValueResult>();
            }
        }

        public class GetByKeyQueryValidator : AbstractValidator<GetByKeyQuery>
        {
            public GetByKeyQueryValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
