namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Validates that a string property, field or parameter contains a valid Bulgarian IBAN in electronic format.
/// <see langword="null"/> is considered valid; add <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> to require a value.
/// </summary>
/// <seealso cref="BgIban.IsValid(string?)"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BgIbanAttribute : BgIdentifierAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BgIbanAttribute"/> class with the default error message.
    /// </summary>
    public BgIbanAttribute()
        : base("The {0} field is not a valid Bulgarian IBAN.")
    {
    }

    private protected override bool IsValidIdentifier(string value) => BgIban.IsValid(value);
}
