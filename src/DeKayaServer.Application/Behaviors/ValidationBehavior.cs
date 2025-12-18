using FluentValidation;
using FluentValidation.Results;
using TS.MediatR;

namespace DeKayaServer.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    //Asagidaki tum kodlar FluentValidation kontorleri icin yazilmistir.
    //EN : All the code below is written for FluentValidation controls.
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    //Bu metod, gelen istegi dogrulamak icin kullanilir.Eger validation varsa tek tek kontrol eder. Eger hatalar varsa, bir ValidationException firlatir.
    //EN : This method is used to validate the incoming request. If there are validations, it checks them one by one. If there are errors, it throws a ValidationException.
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var errorDictionary = _validators
            .Select(s => s.Validate(context))
            .SelectMany(s => s.Errors)
            .Where(s => s != null)
            .GroupBy(
            s => s.PropertyName,
            s => s.ErrorMessage, (propertyName, errorMessage) => new
            {
                Key = propertyName,
                Values = errorMessage.Distinct().ToArray()
            })
            .ToDictionary(s => s.Key, s => s.Values[0]);

        if (errorDictionary.Any())
        {
            var errors = errorDictionary.Select(s => new ValidationFailure
            {
                PropertyName = s.Value,
                ErrorCode = s.Key
            });
            throw new ValidationException(errors);
        }

        return await next();
    }
}
