using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections
{
    public class DeleteConnectionCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public class DeleteconnectionCommandHandler : IRequestHandler<DeleteConnectionCommand, bool>
        {
            private readonly EtcdManagerDataContext _dataContext;
            private readonly IUserPrincipalService _userPrincipalService;

            public DeleteconnectionCommandHandler(EtcdManagerDataContext dataContext, IUserPrincipalService userPrincipalService)
            {
                _dataContext = dataContext;
                _userPrincipalService = userPrincipalService;
            }

            public async Task<bool> Handle(DeleteConnectionCommand request, CancellationToken cancellationToken)
            {
                var ownerId = _userPrincipalService.Id;   
                var connection = await _dataContext.EtcdConnections.FirstOrDefaultAsync(x => x.Id == request.Id && x.OwnerId == ownerId);
                if (connection != null)
                {
                    _dataContext.EtcdConnections.Remove(connection);
                    _dataContext.SaveChanges();
                    return true;
                }
                throw new Exception("Connection not found");
            }
        }

        public class DeleteConnectionCommandValidator : AbstractValidator<DeleteConnectionCommand>
        {
            public DeleteConnectionCommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }
    }
}
