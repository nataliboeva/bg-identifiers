using System.Diagnostics.CodeAnalysis;

namespace BgIdentifiers;

/// <summary>
/// Validates Bulgarian VAT identification numbers (ИН по ДДС) in the EU format used by VIES:
/// the <c>BG</c> prefix followed by 9 or 10 digits.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description><c>BG</c> + 9 digits: the EIK of a legal entity or sole trader. 13-digit branch
/// codes are never VAT numbers, because branches are registered under their parent's EIK.</description></item>
/// <item><description><c>BG</c> + 10 digits: the EGN of a Bulgarian citizen or the LNCh of a foreigner
/// registered for VAT as an individual.</description></item>
/// </list>
/// <para>
/// The National Revenue Agency can also register foreigners without an LNCh under a 10-digit
/// "official number". No authoritative public description of its check digit exists, so such
/// numbers are not recognized and will be reported as invalid.
/// </para>
/// <para>
/// This method only checks the format and check digits. It does not tell you whether the number
/// is currently registered - use the European Commission's VIES service for that.
/// </para>
/// </remarks>
public static class BgVat
{
    /// <summary>The country prefix of Bulgarian VAT numbers.</summary>
    public const string Prefix = "BG";

    /// <summary>
    /// Checks whether <paramref name="vat"/> is a valid Bulgarian VAT number such as <c>BG175074752</c>.
    /// </summary>
    /// <param name="vat">The value to check. The <c>BG</c> prefix is required and must be upper case;
    /// the value is not trimmed and may not contain spaces.</param>
    /// <returns><see langword="true"/> if the value is a valid VAT number; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> vat)
    {
        if (!vat.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var number = vat[Prefix.Length..];
        return number.Length switch
        {
            Eik.Length => Eik.IsValid(number),
            Egn.Length => Egn.IsValid(number) || Lnch.IsValid(number),
            _ => false,
        };
    }

    /// <inheritdoc cref="IsValid(ReadOnlySpan{char})"/>
    public static bool IsValid([NotNullWhen(true)] string? vat) => vat is not null && IsValid(vat.AsSpan());
}
