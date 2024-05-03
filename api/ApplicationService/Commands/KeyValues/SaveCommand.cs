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

        public class SaveCommandHandler : IRequestHandler<SaveCommand, bool>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IEtcdService _etcdService;
            private readonly IUserPrincipalService _userPrincipalService;

            public SaveCommandHandler(EtcdManagerDataContext dataContext, IEtcdService etcdService, IUserPrincipalService userPrincipalService)
            {
                _dataContext = dataContext;
                _etcdService = etcdService;
                _userPrincipalService = userPrincipalService;
            }

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
