using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues
{
    public class GetRevisionQuery: IRequest<List<KeyVersion>>
    {
        public string Key { get; set; } = null!;
        public int EtcdConnectionId { get; set; }

        public class GetRevisionQueryHandler(
            IEtcdService _etcdService,
            EtcdManagerDataContext _dataContext,
            IUserPrincipalService _userPrincipalService
            ): IRequestHandler<GetRevisionQuery, List<KeyVersion>>
        {
            public async Task<List<KeyVersion>> Handle(GetRevisionQuery request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                var result = await _etcdService.GetRevisions(request.Key, connectionSetting);
                return result;
            }
        }

        public class GetRevisionQueryValidator: AbstractValidator<GetRevisionQuery>
        {
            public GetRevisionQueryValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
