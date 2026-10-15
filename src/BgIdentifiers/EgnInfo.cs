namespace BgIdentifiers;

/// <summary>
/// The information that can be reliably decoded from a valid EGN.
/// </summary>
/// <param name="BirthDate">The date of birth encoded in the first six digits.</param>
/// <param name="Gender">The gender encoded in the ninth digit.</param>
public readonly record struct EgnInfo(DateOnly BirthDate, Gender Gender);
