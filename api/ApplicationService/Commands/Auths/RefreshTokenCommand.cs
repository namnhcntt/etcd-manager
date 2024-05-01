using EtcdManager.API.Domain.Services;
using Mapster;
using MediatR;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;

namespace EtcdManager.API.ApplicationService.Commands.Auths
{
    public class RefreshTokenCommand: IRequest<LoginCommandResult>
    {
        public string RefreshToken { get; set; }

        public class RefreshTokenCommandHandler: IRequestHandler<RefreshTokenCommand, LoginCommandResult>
        {
            private readonly IConfiguration _configuration;
            private readonly ITokenService _tokenService;

            public RefreshTokenCommandHandler(IConfiguration configuration, ITokenService tokenService)
            {
                _configuration = configuration;
                _tokenService = tokenService;
            }

            public async Task<LoginCommandResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            {
                var tokenData = await _tokenService.RefreshToken(request.RefreshToken);
                return tokenData.Adapt<LoginCommandResult>();
            }
        }
    }
}
