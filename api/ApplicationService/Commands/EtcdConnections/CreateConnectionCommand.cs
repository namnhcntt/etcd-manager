using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using Mapster;
using MediatR;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections
{
    public class CreateConnectionCommand : IRequest<bool>
    {
        public string Name { get; set; } = null!;
        public string Server { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PermissionUsers { get; set; }
        public bool EnableAuthenticated { get; set; }
        public bool Insecure { get; set; }
        public string? AgentDomain { get; set; }

        public class CreateConnectionCommandHandler : IRequestHandler<CreateConnectionCommand, bool>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public CreateConnectionCommandHandler(EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<bool> Handle(CreateConnectionCommand request, CancellationToken cancellationToken)
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
}
