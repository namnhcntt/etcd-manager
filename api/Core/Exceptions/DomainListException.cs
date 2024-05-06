using EtcdManager.API.Core.Models;
using FluentValidation.Results;
using System.Runtime.Serialization;

namespace EtcdManager.API.Core.Exceptions
{
    /// <summary>
    /// Exception for domain list errors
    /// </summary>
    [Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class DomainListException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public Dictionary<(string code, string? message), string?> Errors { get; private set; } =
          new Dictionary<(string code, string? message), string?>();

        public DomainListException(Dictionary<(string code, string? message), string?> errors)
        {
            Errors = errors;
        }

        public DomainListException(IEnumerable<ValidationFailure>? errors)
        {
            if (errors is null)
            {
                return;
            }

            foreach (var error in errors)
            {
                Errors.Add((error.ErrorCode, error.ErrorMessage), error.PropertyName);
            }
        }

        public DomainListException(IEnumerable<ErrorDetail>? errors)
        {
            if (errors is null)
            {
                return;
            }

            foreach (var error in errors)
            {
                Errors.Add((error.ErrorCode, error.ErrorMessage), error.PropertyName);
            }
        }

        public DomainListException(SerializationInfo info, StreamingContext context)
          : base(info, context) { }
    }
}
