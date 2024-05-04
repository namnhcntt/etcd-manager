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

        public class GetByKeyPrefixQueryHandler : IRequestHandler<GetByKeyPrefixQuery, List<KeyVersion>>
        {
            private readonly IEtcdService _etcdService;
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public GetByKeyPrefixQueryHandler(IEtcdService etcdService, EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _etcdService = etcdService;
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

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
