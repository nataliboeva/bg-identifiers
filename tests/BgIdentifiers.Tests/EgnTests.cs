namespace BgIdentifiers.Tests;

public class EgnTests
{
    [Theory]
    // Examples from python-stdnum and django-localflavor test suites.
    [InlineData("8032056031")]
    [InlineData("7523169263")]
    [InlineData("8001010008")]
    [InlineData("7501020018")]
    [InlineData("7552010005")]
    [InlineData("7542011030")]
    [InlineData("0010100100")]
    // Examples from the Bulgarian and English Wikipedia articles.
    [InlineData("0847038794")]
    [InlineData("1547121580")]
    [InlineData("0011113966")]
    public void IsValid_returns_true_for_published_examples(string egn)
    {
        Egn.IsValid(egn).ShouldBeTrue();
        Egn.IsValid(egn.AsSpan()).ShouldBeTrue();
    }

    [Theory]
    [InlineData("8032056031", 1880, 12, 5, Gender.Female)]
    [InlineData("7523169263", 1875, 3, 16, Gender.Male)]
    [InlineData("8001010008", 1980, 1, 1, Gender.Male)]
    [InlineData("7501020018", 1975, 1, 2, Gender.Female)]
    [InlineData("7552010005", 2075, 12, 1, Gender.Male)]
    [InlineData("7542011030", 2075, 2, 1, Gender.Female)]
    [InlineData("0847038794", 2008, 7, 3, Gender.Female)]
    public void TryParse_decodes_birth_date_and_gender(string egn, int year, int month, int day, Gender gender)
    {
        Egn.TryParse(egn, out var info).ShouldBeTrue();

        info.ShouldBe(new EgnInfo(new DateOnly(year, month, day), gender));
    }

    [Theory]
    [InlineData("0021010008", 1800, 1, 1)]
    [InlineData("9932310006", 1899, 12, 31)]
    [InlineData("0001010003", 1900, 1, 1)]
    [InlineData("9901010002", 1999, 1, 1)]
    [InlineData("0041010002", 2000, 1, 1)]
    [InlineData("9952310000", 2099, 12, 31)]
    public void TryParse_handles_century_boundaries(string egn, int year, int month, int day)
    {
        Egn.TryParse(egn, out var info).ShouldBeTrue();

        info.BirthDate.ShouldBe(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData("9901010002", Gender.Male)]
    [InlineData("9901010018", Gender.Female)]
    public void TryParse_reads_gender_from_parity_of_ninth_digit(string egn, Gender expected)
    {
        Egn.TryParse(egn, out var info).ShouldBeTrue();

        info.Gender.ShouldBe(expected);
    }

    [Fact]
    public void IsValid_accepts_zero_check_digit_when_remainder_is_ten()
    {
        // The weighted sum of 900101000 leaves a remainder of 10, which maps to 0.
        Egn.IsValid("9001010000").ShouldBeTrue();
        Egn.IsValid("9001010001").ShouldBeFalse();
    }

    [Theory]
    [InlineData("8032056030")]
    [InlineData("8032056032")]
    [InlineData("7111042925")] // a valid LNCh, but not a valid EGN
    public void IsValid_returns_false_for_wrong_check_digit(string egn)
    {
        Egn.IsValid(egn).ShouldBeFalse();
    }

    [Theory]
    // Every value below has a correct check digit, so only the date makes it invalid.
    [InlineData("9000010005")] // month 00
    [InlineData("9013010006")] // month 13
    [InlineData("9020010000")] // month 20
    [InlineData("9033010000")] // month 33
    [InlineData("9040010004")] // month 40
    [InlineData("9053010005")] // month 53
    [InlineData("9001000001")] // day 00
    [InlineData("9001320005")] // day 32
    [InlineData("9004310000")] // 31 April
    [InlineData("0142290004")] // 29 February 2001
    [InlineData("0002290070")] // 29 February 1900 (not a leap year)
    [InlineData("1234567890")]
    public void IsValid_returns_false_for_impossible_dates(string egn)
    {
        Egn.IsValid(egn).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_accepts_29_february_in_leap_year_2000()
    {
        Egn.TryParse("0042290090", out var info).ShouldBeTrue();

        info.BirthDate.ShouldBe(new DateOnly(2000, 2, 29));
    }

    [Theory]
    [InlineData("")]
    [InlineData("803205603")]
    [InlineData("80320560311")]
    [InlineData(" 8032056031")]
    [InlineData("8032056031 ")]
    [InlineData("803205 6031")]
    [InlineData("80320A6031")]
    [InlineData("8032-56031")]
    [InlineData("８０３２０５６０３１")] // full-width digits
    [InlineData("٨٠٣٢٠٥٦٠٣١")] // Arabic-Indic digits
    public void IsValid_returns_false_for_malformed_input(string egn)
    {
        Egn.IsValid(egn).ShouldBeFalse();
    }

    [Fact]
    public void Null_input_is_invalid()
    {
        Egn.IsValid((string?)null).ShouldBeFalse();
        Egn.TryParse((string?)null, out var info).ShouldBeFalse();
        info.ShouldBe(default);
    }

    [Fact]
    public void TryParse_returns_default_info_for_invalid_input()
    {
        Egn.TryParse("8032056030", out var info).ShouldBeFalse();

        info.ShouldBe(default);
    }

    [Fact]
    public void TryParse_works_on_a_slice_without_allocating_a_string()
    {
        var text = "EGN: 8032056031;".AsSpan();

        Egn.TryParse(text.Slice(5, Egn.Length), out var info).ShouldBeTrue();
        info.BirthDate.ShouldBe(new DateOnly(1880, 12, 5));
    }
}
