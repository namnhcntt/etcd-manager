using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.KeyValues
{
    public class DeleteKeyCommand: IRequest<bool>
    {
        public string Key { get; set; } = null!;
        public bool DeleteRecursive { get; set; }
        public int EtcdConnectionId { get; set; }

        public class DeleteKeyCommandHandler : IRequestHandler<DeleteKeyCommand, bool>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;
            private readonly IEtcdService _etcdService;

            public DeleteKeyCommandHandler(
                EtcdManagerDataContext dataContext,
                IUserPrincipalService userPrincipalService,
                IEtcdService etcdService
                )
            {
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
                _etcdService = etcdService;
            }

            public async Task<bool> Handle(DeleteKeyCommand request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                await _etcdService.Delete(request.Key, request.DeleteRecursive, connectionSetting);
                return true;
            }
        }

        public class DeleteKeyCommandValidator : AbstractValidator<DeleteKeyCommand>
        {
            public DeleteKeyCommandValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
