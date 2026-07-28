using FluentValidation;

using Microsoft.Extensions.Options;

namespace Pessoas.Integracao.Sync.Infrastructure.Common;

public class FluentValidateOptions<T>(IValidator<T> validator) : IValidateOptions<T> where T : class
{
    private readonly IValidator<T> _validator = validator;

    public ValidateOptionsResult Validate(string? name, T options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail($"{typeof(T).Name} options not found.");
        }

        var result = _validator.Validate(options);
        if (result.IsValid) return ValidateOptionsResult.Success;

        var errors = string.Join(" ", result.Errors.Select(e => e.ErrorMessage));
        return ValidateOptionsResult.Fail(errors);
    }
}
