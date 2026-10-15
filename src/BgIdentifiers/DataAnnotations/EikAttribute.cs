namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Validates that a string property, field or parameter contains a valid 9-digit or 13-digit EIK/BULSTAT code.
/// <see langword="null"/> is considered valid; add <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> to require a value.
/// </summary>
/// <seealso cref="Eik.IsValid(string?)"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class EikAttribute : BgIdentifierAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EikAttribute"/> class with the default error message.
    /// </summary>
    public EikAttribute()
        : base("The {0} field is not a valid EIK/BULSTAT code.")
    {
    }

    private protected override bool IsValidIdentifier(string value) => Eik.IsValid(value);
}
