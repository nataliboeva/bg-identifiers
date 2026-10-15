using System.ComponentModel.DataAnnotations;

namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Base class for the validation attributes in this package.
/// </summary>
/// <remarks>
/// <see langword="null"/> is considered valid, so that optional properties can use the attribute
/// on its own - combine it with <see cref="RequiredAttribute"/> to make a value mandatory.
/// Values that are not strings are always invalid.
/// </remarks>
public abstract class BgIdentifierAttribute : ValidationAttribute
{
    // private protected: the attribute types are public, but only this assembly can add new ones.
    private protected BgIdentifierAttribute(string defaultErrorMessage)
        : base(defaultErrorMessage)
    {
    }

    /// <inheritdoc/>
    public override bool IsValid(object? value) => value switch
    {
        null => true,
        string text => IsValidIdentifier(text),
        _ => false,
    };

    private protected abstract bool IsValidIdentifier(string value);
}
