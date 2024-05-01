using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections
{
    public class UpdateConnectionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Server { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PermissionUsers { get; set; }
        public bool EnableAuthenticated { get; set; }
        public bool Insecure { get; set; }
        public string? AgentDomain { get; set; }

        public class UpdateConnectionCommandHandler : IRequestHandler<UpdateConnectionCommand, bool>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public UpdateConnectionCommandHandler(EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<bool> Handle(UpdateConnectionCommand request, CancellationToken cancellationToken)
            {
                var ownerId = _userPrincipalService.Id;
                var connection = await _dataContext.EtcdConnections.FirstOrDefaultAsync(x => x.Id == request.Id && x.OwnerId == ownerId);
                if (connection != null)
                {
                    connection.Server = request.Server;
                    connection.UserName = request.UserName;
                    connection.Password = request.Password;
                    connection.PermissionUsers = request.PermissionUsers;
                    connection.EnableAuthenticated = request.EnableAuthenticated;
                    connection.Insecure = request.Insecure;
                    connection.AgentDomain = request.AgentDomain;
                    _dataContext.SaveChanges();
                    return true;
                }
                throw new Exception("Connection not found");
            }
        }

        public class UpdateConnectionCommandValidator : AbstractValidator<UpdateConnectionCommand>
        {
            public UpdateConnectionCommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Server).NotEmpty();
            }
        }
    }
}
