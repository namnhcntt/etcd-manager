using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using Mapster;
using MediatR;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections;

public class CreateConnectionCommand : IRequest<bool>
{
    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PermissionUsers { get; set; }
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public string? AgentDomain { get; set; }

    public class CreateConnectionCommandHandler(
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService
    ) : IRequestHandler<CreateConnectionCommand, bool>
    {
        public async Task<bool> Handle(
            CreateConnectionCommand request,
            CancellationToken cancellationToken
        )
        {
            var connection = request.Adapt<EtcdConnection>();
            connection.CreatedAt = DateTime.UtcNow;
            connection.OwnerId = _userPrincipalService.Id;

            await _dataContext.EtcdConnections.AddAsync(connection);
            await _dataContext.SaveChangesAsync();

            return true;
        }
    }

    public class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommand>
    {
        public CreateConnectionCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Server).NotEmpty();
        }
    }
}
