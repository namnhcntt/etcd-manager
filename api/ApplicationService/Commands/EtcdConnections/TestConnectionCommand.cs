using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Infrastructure.Etcd;
using FluentValidation;
using MediatR;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections;

public class TestConnectionCommand : IRequest<bool>
{
    public string Server { get; set; } = null!;
    public bool EnableAuthenticated { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool Insecure { get; set; }
    public string Host => EtcdServerParser.ParseHostAndPort(Server, Insecure).Host;

    public string Port => EtcdServerParser.ParseHostAndPort(Server, Insecure).Port;

    public class TestConnectionCommandHandler(IEtcdService _etcdService)
        : IRequestHandler<TestConnectionCommand, bool>
    {
        public Task<bool> Handle(TestConnectionCommand request, CancellationToken cancellationToken)
        {
            return _etcdService.TestConnection(
                request.Host,
                request.Port,
                request.EnableAuthenticated,
                request.Insecure,
                request.Username,
                request.Password
            );
        }
    }

    public class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommand>
    {
        public TestConnectionCommandValidator(IConfiguration configuration)
        {
            var blockPrivateNetworks = configuration.GetValue<bool>("Etcd:BlockPrivateNetworks");
            RuleFor(x => x.Server)
                .NotEmpty()
                .Must(server => EtcdServerAddressValidator.IsAllowed(server, blockPrivateNetworks))
                .WithMessage("Server address is not allowed.");
            // rule for username, password not empty if enableAuthenticated = true
            RuleFor(x => x.Username).NotEmpty().When(x => x.EnableAuthenticated);
            RuleFor(x => x.Password).NotEmpty().When(x => x.EnableAuthenticated);
        }
    }
}
