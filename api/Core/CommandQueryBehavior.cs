using EtcdManager.API.Core.Exceptions;
using FluentValidation;
using MediatR;

namespace EtcdManager.API.Core
{
    /// <summary>
    /// MediatR behavior, that run Fluent validation before command execution
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }
            var context = new ValidationContext<TRequest>(request);
            var errors = _validators
              .Select(x => x.Validate(context))
              .Where(x => !x.IsValid)
              .SelectMany(x => x.Errors)
              .DistinctBy(
                x =>
                  new
                  {
                      x.ErrorMessage,
                      x.PropertyName,
                      x.ErrorCode
                  }
              )
              .ToList();
            if (errors.Any())
            {
                throw new DomainListException(errors);
            }
            return await next();
        }
    }
}
