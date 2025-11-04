using CoreBanking.APP.Common.Models;
using FluentValidation;
using MediatR;
namespace CoreBanking.APP.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : Result

{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Any())
        {
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType)
            {
                var genericTYpe = resultType.GetGenericArguments()[0];
                var failureMehtod = typeof(Result<>).MakeGenericType(genericTYpe).GetMethod(nameof(Result<object>.Failure))!;
                var errors = failures.Select(f => f.ErrorMessage).ToArray();
                return (TResponse)failureMehtod.Invoke(null, [errors])!;
            }

        }
        return await next();

    }

}
