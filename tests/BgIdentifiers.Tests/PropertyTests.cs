using CsCheck;

namespace BgIdentifiers.Tests;

/// <summary>
/// Property-based tests: instead of hand-picked examples, CsCheck generates thousands of
/// inputs (and shrinks any failure to a minimal example).
/// </summary>
public class PropertyTests
{
    private const long Iterations = 10_000;

    private static readonly Gen<DateOnly> BirthDates = Gen.DateOnly[Egn.MinBirthDate, Egn.MaxBirthDate];
    private static readonly Gen<int> Seeds = Gen.Int;
    private static readonly Gen<int> NonZeroDigitDeltas = Gen.Int[1, 9];

    [Fact]
    public void Generated_egn_is_valid_and_decodes_to_the_requested_date_and_gender()
    {
        Gen.Select(BirthDates, Gen.Enum<Gender>(), Seeds).Sample((birthDate, gender, seed) =>
        {
            var egn = Egn.Generate(birthDate, gender, new Random(seed));

            return Egn.TryParse(egn, out var info) && info == new EgnInfo(birthDate, gender);
        }, iter: Iterations);
    }

    [Fact]
    public void Changing_the_egn_check_digit_makes_it_invalid()
    {
        Gen.Select(BirthDates, Gen.Enum<Gender>(), Seeds, NonZeroDigitDeltas).Sample((birthDate, gender, seed, delta) =>
        {
            var egn = Egn.Generate(birthDate, gender, new Random(seed));

            return !Egn.IsValid(ReplaceDigit(egn, 9, delta));
        }, iter: Iterations);
    }

    [Fact]
    public void Lnch_detects_every_single_digit_error()
    {
        // All LNCh weights are coprime with 10, so any one changed digit changes the checksum.
        Gen.Select(Seeds, Gen.Int[0, 9], NonZeroDigitDeltas).Sample((seed, position, delta) =>
        {
            var lnch = Lnch.Generate(new Random(seed));

            return Lnch.IsValid(lnch) && !Lnch.IsValid(ReplaceDigit(lnch, position, delta));
        }, iter: Iterations);
    }

    [Fact]
    public void Generated_eik_is_valid_and_changing_its_check_digit_makes_it_invalid()
    {
        Gen.Select(Seeds, NonZeroDigitDeltas).Sample((seed, delta) =>
        {
            var eik = Eik.Generate(new Random(seed));

            return eik.Length == Eik.Length
                && Eik.IsValid(eik)
                && !Eik.IsValid(ReplaceDigit(eik, 8, delta));
        }, iter: Iterations);
    }

    [Fact]
    public void Generated_branch_eik_is_valid_and_changing_its_last_digit_makes_it_invalid()
    {
        Gen.Select(Seeds, NonZeroDigitDeltas).Sample((seed, delta) =>
        {
            var eik = Eik.GenerateBranch(new Random(seed));

            return eik.Length == Eik.BranchLength
                && Eik.IsValid(eik)
                && !Eik.IsValid(ReplaceDigit(eik, 12, delta));
        }, iter: Iterations);
    }

    [Fact]
    public void Vat_number_is_bg_followed_by_a_valid_eik_egn_or_lnch()
    {
        Gen.Select(BirthDates, Gen.Enum<Gender>(), Seeds).Sample((birthDate, gender, seed) =>
        {
            var random = new Random(seed);

            return BgVat.IsValid("BG" + Eik.Generate(random))
                && BgVat.IsValid("BG" + Egn.Generate(birthDate, gender, random))
                && BgVat.IsValid("BG" + Lnch.Generate(random))
                && !BgVat.IsValid("BG" + Eik.GenerateBranch(random));
        }, iter: Iterations);
    }

    [Fact]
    public void Iban_detects_every_single_character_substitution()
    {
        Gen.Select(Seeds, Gen.Int[4, BgIban.Length - 1], Gen.Int[1, 25]).Sample((seed, position, delta) =>
        {
            var iban = BgIban.Generate(new Random(seed)).ToCharArray();
            var original = iban[position];

            // Swap a letter for another letter and a digit for another digit, so the structure stays valid
            // and only the mod 97 check can catch the error.
            iban[position] = char.IsAsciiDigit(original)
                ? (char)('0' + ((original - '0' + (delta % 9) + 1) % 10))
                : (char)('A' + ((original - 'A' + delta) % 26));

            return !BgIban.IsValid(new string(iban));
        }, iter: Iterations);
    }

    [Fact]
    public void Iban_detects_swapped_adjacent_digits()
    {
        Gen.Select(Seeds, Gen.Int[8, BgIban.Length - 2]).Sample((seed, position) =>
        {
            var iban = BgIban.Generate(new Random(seed)).ToCharArray();
            if (iban[position] == iban[position + 1])
            {
                return true; // swapping equal digits changes nothing
            }

            (iban[position], iban[position + 1]) = (iban[position + 1], iban[position]);
            return !BgIban.IsValid(new string(iban));
        }, iter: Iterations);
    }

    [Fact]
    public void Validators_never_throw_and_only_accept_ascii_input_of_the_right_length()
    {
        var digitStrings = Gen.Char['0', '9'].Array[0, 24].Select(chars => new string(chars));
        var inputs = Gen.OneOf(Gen.String, digitStrings, digitStrings.Select(s => "BG" + s));

        inputs.Sample(input =>
        {
            if (Egn.IsValid(input) && !IsAsciiDigits(input, Egn.Length))
            {
                return false;
            }

            if (Lnch.IsValid(input) && !IsAsciiDigits(input, Lnch.Length))
            {
                return false;
            }

            if (Eik.IsValid(input) && !(IsAsciiDigits(input, Eik.Length) || IsAsciiDigits(input, Eik.BranchLength)))
            {
                return false;
            }

            if (BgVat.IsValid(input) && !input.StartsWith("BG", StringComparison.Ordinal))
            {
                return false;
            }

            return !BgIban.IsValid(input) || input.Length == BgIban.Length;
        }, iter: Iterations);
    }

    [Fact]
    public void Egn_accepts_exactly_the_real_calendar_dates()
    {
        // Exhaustively checks all 1,000,000 YYMMDD combinations (with a correct check digit)
        // against an independent oracle built on DateOnly.TryParseExact.
        var mismatches = new List<string>();

        for (var yy = 0; yy < 100; yy++)
        {
            for (var mm = 0; mm < 100; mm++)
            {
                for (var dd = 0; dd < 100; dd++)
                {
                    var firstNine = $"{yy:00}{mm:00}{dd:00}000";
                    var egn = firstNine + EgnCheckDigit(firstNine);
                    var expected = ExpectedBirthDate(yy, mm, dd);

                    var isValid = Egn.TryParse(egn, out var info);
                    if (isValid != expected.HasValue || (isValid && info.BirthDate != expected))
                    {
                        mismatches.Add(egn);
                    }
                }
            }
        }

        mismatches.ShouldBeEmpty();
    }

    private static string ReplaceDigit(string value, int position, int delta)
    {
        var chars = value.ToCharArray();
        chars[position] = (char)('0' + ((chars[position] - '0' + delta) % 10));
        return new string(chars);
    }

    private static bool IsAsciiDigits(string value, int length) =>
        value.Length == length && value.All(char.IsAsciiDigit);

    private static int EgnCheckDigit(string firstNine)
    {
        int[] weights = [2, 4, 8, 5, 10, 9, 7, 3, 6];
        var remainder = firstNine.Select((c, i) => (c - '0') * weights[i]).Sum() % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private static DateOnly? ExpectedBirthDate(int yy, int mm, int dd)
    {
        var (year, month) = mm switch
        {
            > 40 => (2000 + yy, mm - 40),
            > 20 => (1800 + yy, mm - 20),
            _ => (1900 + yy, mm),
        };

        return DateOnly.TryParseExact(
            $"{year:0000}-{month:00}-{dd:00}",
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var date)
            ? date
            : null;
    }
}
