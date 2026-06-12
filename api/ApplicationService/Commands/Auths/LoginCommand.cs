using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;

namespace EtcdManager.API.ApplicationService.Commands.Auths;

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
            EtcdManagerDataContext _dataContext,
            IMemoryCache _memoryCache
        ) : IRequestHandler<LoginCommand, LoginCommandResult>
        {
            // Per-username failed-login lockout. The "login" rate limiter in Program.cs
            // is IP-only; this counter covers distributed attacks against one account.
            private const int MaxFailedAttempts = 5;
            private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);

            // Precomputed hash used to equalize timing when the username does not exist,
            // so both failure paths perform exactly one BCrypt verification.
            private static readonly string DummyPasswordHash = CommonHelper.HashPassword(
                "timing-equalization-dummy-password"
            );

            public async Task<LoginCommandResult> Handle(
                LoginCommand request,
                CancellationToken cancellationToken
            )
            {
                var failedAttemptsKey = $"login_failed_attempts:{request.Username}";
                if (
                    _memoryCache.TryGetValue<int>(failedAttemptsKey, out var failedAttempts)
                    && failedAttempts >= MaxFailedAttempts
                )
                {
                    throw new Exception("Too many failed login attempts. Please try again later.");
                }

                var user = await _dataContext.AppUsers.FirstOrDefaultAsync(x =>
                    x.Username == request.Username,
                    cancellationToken
                );

                var passwordValid = CommonHelper.VerifyPassword(
                    request.Password,
                    user?.Password ?? DummyPasswordHash
                );

                if (user != null && passwordValid)
                {
                    _memoryCache.Remove(failedAttemptsKey);
                    var tokenData = await _tokenService.GenerateJwtTokenData(user.Id, request.Username);
                    return tokenData.Adapt<LoginCommandResult>();
                }

                _memoryCache.Set(failedAttemptsKey, failedAttempts + 1, LockoutWindow);
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
