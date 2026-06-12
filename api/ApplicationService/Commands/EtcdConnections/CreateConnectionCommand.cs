using EtcdManager.API.Core.Helpers;
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
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public string? AgentDomain { get; set; }

    public class CreateConnectionCommandHandler(
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService,
        IPasswordProtectorService _passwordProtector
    ) : IRequestHandler<CreateConnectionCommand, bool>
    {
        public async Task<bool> Handle(
            CreateConnectionCommand request,
            CancellationToken cancellationToken
        )
        {
            var connection = request.Adapt<EtcdConnection>();
            connection.Password = _passwordProtector.Protect(request.Password);
            connection.CreatedAt = DateTime.UtcNow;
            connection.OwnerId = _userPrincipalService.Id;

            await _dataContext.EtcdConnections.AddAsync(connection);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommand>
    {
        public CreateConnectionCommandValidator(IConfiguration configuration)
        {
            var blockPrivateNetworks = configuration.GetValue<bool>("Etcd:BlockPrivateNetworks");
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Server)
                .NotEmpty()
                .Must(server => EtcdServerAddressValidator.IsAllowed(server, blockPrivateNetworks))
                .WithMessage("Server address is not allowed.");
        }
    }
}
