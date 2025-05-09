﻿using EtcdManager.API.Infrastructure.Etcd;
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
    public string Host
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Server))
                return string.Empty;

            var host = Server.Split(':')[0];

            if (!host.StartsWith("https://"))
            {
                if (host.StartsWith("http://"))
                {
                    host = host.Replace("http://", "https://");
                }
                else if (Insecure)
                {
                    host = "http://" + host;
                }
                else
                {
                    host = "https://" + host;
                }
            }

            return host;
        }
    }

    public string Port
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Server))
                return string.Empty;

            var arr = Server.Split(':');
            return arr.Length > 1 ? arr[1] : "2379";
        }
    }

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
        public TestConnectionCommandValidator()
        {
            RuleFor(x => x.Server).NotEmpty();
            // rule for username, password not empty if enableAuthenticated = true
            RuleFor(x => x.Username).NotEmpty().When(x => x.EnableAuthenticated);
            RuleFor(x => x.Password).NotEmpty().When(x => x.EnableAuthenticated);
        }
    }
}
