using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetByKeyPrefixQuery: IRequest<List<KeyVersion>>
    {
        public int EtcdConnectionId { get; set; }
        public string Prefix { get; set; } = null!;

        public class GetByKeyPrefixQueryHandler(
            IEtcdService _etcdService,
            EtcdManagerDataContext _dataContext,
            IUserPrincipalService _userPrincipalService
            ) : IRequestHandler<GetByKeyPrefixQuery, List<KeyVersion>>
        {
            public async Task<List<KeyVersion>> Handle(GetByKeyPrefixQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetByKeyPrefix(request.Prefix, connectionSetting);
                return result;
            }
        }

        public class GetByKeyPrefixQueryValidator: AbstractValidator<GetByKeyPrefixQuery>
        {
            public GetByKeyPrefixQueryValidator()
            {
                RuleFor(x => x.Prefix).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
