using System.Security.Cryptography;
using EtcdManager.API.Domain.Services;
using Microsoft.AspNetCore.DataProtection;

namespace EtcdManager.API.Infrastructure.Authentication;

public class PasswordProtectorService : IPasswordProtectorService
{
    private const string Purpose = "EtcdManager.ConnectionPasswords";

    private readonly IDataProtector _protector;
    private readonly ILogger<PasswordProtectorService> _logger;

    public PasswordProtectorService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<PasswordProtectorService> logger
    )
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string? Protect(string? password)
    {
        return string.IsNullOrEmpty(password) ? password : _protector.Protect(password);
    }

    public string? Unprotect(string? storedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword))
            return storedPassword;

        try
        {
            return _protector.Unprotect(storedPassword);
        }
        catch (CryptographicException)
        {
            // Legacy value stored as plaintext before encryption was introduced —
            // use it as-is; it gets re-protected on the next password update.
            _logger.LogWarning(
                "Stored password could not be unprotected; treating as legacy plaintext. "
                    + "If this is unexpected, Data Protection keys may be missing."
            );
            return storedPassword;
        }
    }
}
