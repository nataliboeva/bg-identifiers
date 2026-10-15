using FluentValidation;
using FluentValidation.Validators;

namespace BgIdentifiers.FluentValidation;

/// <summary>
/// A property validator that delegates to one of the <c>IsValid</c> methods of the core package.
/// <see cref="Name"/> doubles as the error code, e.g. <c>EgnValidator</c>.
/// </summary>
internal sealed class BgIdentifierValidator<T> : PropertyValidator<T, string?>
{
    private readonly Func<string, bool> _isValid;
    private readonly string _defaultMessageTemplate;

    public BgIdentifierValidator(string name, Func<string, bool> isValid, string defaultMessageTemplate)
    {
        Name = name;
        _isValid = isValid;
        _defaultMessageTemplate = defaultMessageTemplate;
    }

    public override string Name { get; }

    // Like FluentValidation's built-in validators, null is left to NotNull()/NotEmpty().
    public override bool IsValid(ValidationContext<T> context, string? value) => value is null || _isValid(value);

    protected override string GetDefaultMessageTemplate(string errorCode) => _defaultMessageTemplate;
}
