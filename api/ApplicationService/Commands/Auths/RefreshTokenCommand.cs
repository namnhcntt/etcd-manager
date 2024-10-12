using EtcdManager.API.Domain.Services;
using Mapster;
using MediatR;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;

namespace EtcdManager.API.ApplicationService.Commands.Auths;

public class RefreshTokenCommand : IRequest<LoginCommandResult>
{
    public string RefreshToken { get; set; } = null!;

    public class RefreshTokenCommandHandler(ITokenService _tokenService)
        : IRequestHandler<RefreshTokenCommand, LoginCommandResult>
    {
        public async Task<LoginCommandResult> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken
        )
        {
            var tokenData = await _tokenService.RefreshToken(request.RefreshToken);
            return tokenData.Adapt<LoginCommandResult>();
        }
    }
}
