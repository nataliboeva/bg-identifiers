namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Validates that a string property, field or parameter contains a valid EGN (personal identification number).
/// <see langword="null"/> is considered valid; add <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> to require a value.
/// </summary>
/// <seealso cref="Egn.IsValid(string?)"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class EgnAttribute : BgIdentifierAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EgnAttribute"/> class with the default error message.
    /// </summary>
    public EgnAttribute()
        : base("The {0} field is not a valid EGN.")
    {
    }

    private protected override bool IsValidIdentifier(string value) => Egn.IsValid(value);
}
