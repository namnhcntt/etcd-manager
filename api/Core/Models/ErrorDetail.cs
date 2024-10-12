namespace EtcdManager.API.Core.Models;

public class ErrorDetail
{
    public string? PropertyName { get; set; }
    public string ErrorCode { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public ErrorDetail() { }

    public ErrorDetail(string? propertyName, string errorCode, string? errorMessage)
    {
        PropertyName = propertyName;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
