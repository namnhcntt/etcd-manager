namespace EtcdManager.API.Domain.Services;

/// <summary>
/// Encrypts/decrypts stored etcd connection passwords at rest.
/// </summary>
public interface IPasswordProtectorService
{
    string? Protect(string? password);
    string? Unprotect(string? storedPassword);
}
