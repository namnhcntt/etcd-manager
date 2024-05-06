using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;

namespace EtcdManager.API.ApplicationService.Commands.Auths
{
    public class LoginCommand : IRequest<LoginCommandResult>
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public class LoginCommandResult
        {
            public string Token { get; set; } = null!;
            public string RefreshToken { get; set; } = null!;
            public int ExpiresIn { get; set; }

            public class LoginCommandHandler(
                ITokenService _tokenService,
                EtcdManagerDataContext _dataContext
                ) : IRequestHandler<LoginCommand, LoginCommandResult>
            {
                public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
                {
                    // validate user
                    // Password hash sha256
                    var hashedPassword = CommonHelper.SHA256Hash(request.Password);
                    var user = await _dataContext.AppUsers.FirstOrDefaultAsync(x => x.Username == request.Username && x.Password == hashedPassword);
                    // generate token
                    if (user != null)
                    {
                        var tokenData = await _tokenService.GenerateJwtTokenData(user.Id, request.Username);
                        return tokenData.Adapt<LoginCommandResult>();
                    }

                    throw new Exception("Invalid username or password");
                }
            }

            public class LoginCommandValidator : AbstractValidator<LoginCommand>
            {
                public LoginCommandValidator()
                {
                    RuleFor(x => x.Username).NotEmpty();
                    RuleFor(x => x.Password).NotEmpty();
                }
            }
        }
    }
}