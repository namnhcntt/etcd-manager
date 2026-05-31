using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.KeyValues;

public class ImportNodesCommand : IRequest<bool>
{
    public KeyValue[] KeyValues { get; set; } = null!;
    public int EtcdConnectionId { get; set; }

    public class ImportNodesCommandHandler(
        IEtcdService _etcdService,
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService
    ) : IRequestHandler<ImportNodesCommand, bool>
    {
        public async Task<bool> Handle(
            ImportNodesCommand request,
            CancellationToken cancellationToken
        )
        {
            var currentUserId = _userPrincipalService.Id;
            var connectionSetting = await _dataContext.EtcdConnections.FirstAsync(x =>
                x.Id == request.EtcdConnectionId && x.OwnerId == currentUserId
            );
            await _etcdService.ImportNodes(request.KeyValues, connectionSetting);
            return true;
        }
    }

    public class ImportNodesCommandValidator : AbstractValidator<ImportNodesCommand>
    {
        private const int MaxImportBatch = 100;

        public ImportNodesCommandValidator()
        {
            RuleFor(x => x.KeyValues).NotEmpty()
                .Must(kv => kv == null || kv.Length <= MaxImportBatch)
                .WithMessage($"Import batch must not exceed {MaxImportBatch} keys per request.");
            RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
        }
    }
}
