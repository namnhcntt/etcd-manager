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
        private const int MaxKeyLength = 1024;
        private const int MaxValueBytes = 1_048_576; // 1MB

        public ImportNodesCommandValidator()
        {
            RuleFor(x => x.KeyValues).NotEmpty()
                .Must(kv => kv == null || kv.Length <= MaxImportBatch)
                .WithMessage($"Import batch must not exceed {MaxImportBatch} keys per request.");
            RuleForEach(x => x.KeyValues).ChildRules(item =>
            {
                item.RuleFor(x => x.Key).NotEmpty().MaximumLength(MaxKeyLength);
                item.RuleFor(x => x.Value)
                    .Must(v => v == null || System.Text.Encoding.UTF8.GetByteCount(v) <= MaxValueBytes)
                    .WithMessage("Each value must be under 1MB.");
            });
            RuleFor(x => x.EtcdConnectionId).GreaterThan(0);
        }
    }
}
