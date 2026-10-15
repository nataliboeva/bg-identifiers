namespace BgIdentifiers.DataAnnotations;

/// <summary>
/// Validates that a string property, field or parameter contains a valid LNCh (personal number of a foreigner).
/// <see langword="null"/> is considered valid; add <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> to require a value.
/// </summary>
/// <seealso cref="Lnch.IsValid(string?)"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class LnchAttribute : BgIdentifierAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LnchAttribute"/> class with the default error message.
    /// </summary>
    public LnchAttribute()
        : base("The {0} field is not a valid LNCh.")
    {
    }

    private protected override bool IsValidIdentifier(string value) => Lnch.IsValid(value);
}
