using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.KeyValues
{
    public class SaveCommand: IRequest<bool>
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public int EtcdConnectionId { get; set; }

        public class SaveCommandHandler(
            EtcdManagerDataContext _dataContext,
            IEtcdService _etcdService,
            IUserPrincipalService _userPrincipalService
            ) : IRequestHandler<SaveCommand, bool>
        {
            public async Task<bool> Handle(SaveCommand request, CancellationToken cancellationToken)
            {
                var currentUserId = _userPrincipalService.Id;
                var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x => x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId);
                await _etcdService.Save(request.Adapt<KeyValue>(), connectionSetting);
                return true;
            }
        }

        public class SaveCommandValidator : AbstractValidator<SaveCommand>
        {
            public SaveCommandValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.Value).NotEmpty();
                RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
            }
        }
    }
}
