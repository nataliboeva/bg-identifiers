namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Validates that a string property, field or parameter contains a valid Bulgarian VAT number such as <c>BG175074752</c>.
/// <see langword="null"/> is considered valid; add <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> to require a value.
/// </summary>
/// <seealso cref="BgVat.IsValid(string?)"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BgVatAttribute : BgIdentifierAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BgVatAttribute"/> class with the default error message.
    /// </summary>
    public BgVatAttribute()
        : base("The {0} field is not a valid Bulgarian VAT number.")
    {
    }

    private protected override bool IsValidIdentifier(string value) => BgVat.IsValid(value);
}
