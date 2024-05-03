using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetRevisionDetailQuery: IRequest<KeyVersion?>
    {
        public string Key { get; set; } = null!;
        public long Revision { get; set; }
        public int EtcdConnectionId { get; set; }

        public class GetRevisionDetailQueryHandler: IRequestHandler<GetRevisionDetailQuery, KeyVersion?>
        {
            private readonly IEtcdService _etcdService;
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public GetRevisionDetailQueryHandler(IEtcdService etcdService, EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _etcdService = etcdService;
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<KeyVersion?> Handle(GetRevisionDetailQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetRevision(request.Key, request.Revision, connectionSetting);
                return result;
            }
        }

        public class GetRevisionDetailQueryValidator: AbstractValidator<GetRevisionDetailQuery>
        {
            public GetRevisionDetailQueryValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.Revision).GreaterThan(0);
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
