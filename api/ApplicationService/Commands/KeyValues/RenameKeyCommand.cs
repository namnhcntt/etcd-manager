using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.KeyValues;

public class RenameKeyCommand : IRequest<bool>
{
    public string OldKey { get; set; } = null!;
    public string NewKey { get; set; } = null!;
    public int EtcdConnectionId { get; set; }

    public class RenameKeyCommandHandler(
        IEtcdService _etcdService,
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService
    ) : IRequestHandler<RenameKeyCommand, bool>
    {
        public async Task<bool> Handle(
            RenameKeyCommand request,
            CancellationToken cancellationToken
        )
        {
            var currentUserId = _userPrincipalService.Id;
            var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x =>
                x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId
            );
            await _etcdService.RenameKey(request.OldKey, request.NewKey, connectionSetting);
            return true;
        }
    }

    public class RenameKeyCommandValidator : AbstractValidator<RenameKeyCommand>
    {
        public RenameKeyCommandValidator()
        {
            RuleFor(x => x.OldKey).NotEmpty();
            RuleFor(x => x.NewKey).NotEmpty();
            RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
        }
    }
}
