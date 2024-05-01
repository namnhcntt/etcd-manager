using FluentValidation;
using MediatR;

namespace EtcdManager.API.ApplicationService.Commands.EtcdConnections
{
    public class TestConnectionCommand : IRequest<bool>
    {
        public int ConnectionId { get; set; }

        public class TestConnectionCommandHandler : IRequestHandler<TestConnectionCommand, bool>
        {
            public async Task<bool> Handle(TestConnectionCommand request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommand>
        {
            public TestConnectionCommandValidator()
            {
                RuleFor(x => x.ConnectionId).GreaterThan(0);
            }
        }
    }
}
