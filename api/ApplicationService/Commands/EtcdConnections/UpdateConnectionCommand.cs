using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections;

public class UpdateConnectionCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public string? AgentDomain { get; set; }

    public class UpdateConnectionCommandHandler(
        EtcdManagerDataContext _dataContext,
        IUserPrincipalService _userPrincipalService,
        IPasswordProtectorService _passwordProtector
    ) : IRequestHandler<UpdateConnectionCommand, bool>
    {
        public async Task<bool> Handle(
            UpdateConnectionCommand request,
            CancellationToken cancellationToken
        )
        {
            var ownerId = _userPrincipalService.Id;
            var connection = await _dataContext.EtcdConnections.FirstOrDefaultAsync(x =>
                x.Id == request.Id && x.OwnerId == ownerId
            );
            if (connection != null)
            {
                connection.Name = request.Name;
                connection.Server = request.Server;
                connection.Username = request.Username;
                // Keep the stored password when the request does not provide a new one.
                if (!string.IsNullOrEmpty(request.Password))
                {
                    connection.Password = _passwordProtector.Protect(request.Password);
                }
                connection.EnableAuthenticated = request.EnableAuthenticated;
                connection.Insecure = request.Insecure;
                connection.AgentDomain = request.AgentDomain;
                await _dataContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            throw new Exception("Connection not found");
        }
    }

    public class UpdateConnectionCommandValidator : AbstractValidator<UpdateConnectionCommand>
    {
        public UpdateConnectionCommandValidator(IConfiguration configuration)
        {
            var blockPrivateNetworks = configuration.GetValue<bool>("Etcd:BlockPrivateNetworks");
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Server)
                .NotEmpty()
                .Must(server => EtcdServerAddressValidator.IsAllowed(server, blockPrivateNetworks))
                .WithMessage("Server address is not allowed.");
        }
    }
}
