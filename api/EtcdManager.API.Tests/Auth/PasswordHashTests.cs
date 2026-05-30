using EtcdManager.API.Core.Helpers;
using Xunit;

namespace EtcdManager.API.Tests.Auth;

public class PasswordHashTests
{
    [Fact]
    public void HashPassword_SameInput_VerifyReturnsTrue()
    {
        var hash1 = CommonHelper.HashPassword("mypassword");
        var hash2 = CommonHelper.HashPassword("mypassword");
        Assert.True(CommonHelper.VerifyPassword("mypassword", hash1));
        Assert.True(CommonHelper.VerifyPassword("mypassword", hash2));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = CommonHelper.HashPassword("correctpassword");
        Assert.False(CommonHelper.VerifyPassword("wrongpassword", hash));
    }

    [Fact]
    public void VerifyPassword_EmptyString_ReturnsFalse()
    {
        var hash = CommonHelper.HashPassword("correctpassword");
        Assert.False(CommonHelper.VerifyPassword("", hash));
    }
}
