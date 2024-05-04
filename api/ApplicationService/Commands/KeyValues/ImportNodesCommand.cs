using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.KeyValues
{
    public class ImportNodesCommand: IRequest<bool>
    {
        public KeyValue[] KeyValues { get; set; } = null!;
        public int EtcdConnectionId { get; set; }


        public class  ImportNodesCommandHandler: IRequestHandler<ImportNodesCommand, bool>
        {
            private readonly IEtcdService _etcdService;
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public ImportNodesCommandHandler(IEtcdService etcdService, EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)  
            {
                _etcdService = etcdService;
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }
            public async Task<bool> Handle(ImportNodesCommand request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                await _etcdService.ImportNodes(request.KeyValues, connectionSetting);
                return true;
            }
        }

        public class ImportNodesCommandValidator: AbstractValidator<ImportNodesCommand>
        {
            public ImportNodesCommandValidator()
            {
                RuleFor(x => x.KeyValues).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }

    }
}
