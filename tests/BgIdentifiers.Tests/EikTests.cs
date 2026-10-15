namespace BgIdentifiers.Tests;

public class EikTests
{
    [Theory]
    // 9-digit examples from python-stdnum and django-localflavor.
    [InlineData("175074752")]
    [InlineData("831919536")]
    [InlineData("121887948")]
    [InlineData("175015558")]
    [InlineData("175223064")]
    [InlineData("000565359")]
    [InlineData("176040023")]
    [InlineData("121817309")]
    [InlineData("104453698")]
    public void IsValid_returns_true_for_published_9_digit_examples(string eik)
    {
        Eik.IsValid(eik).ShouldBeTrue();
        Eik.IsValid(eik.AsSpan()).ShouldBeTrue();
    }

    [Theory]
    // 13-digit branch examples from django-localflavor; each extends a 9-digit example above.
    [InlineData("0005653590079")]
    [InlineData("1760400230151")]
    [InlineData("1218173090603")]
    [InlineData("1044536985268")]
    public void IsValid_returns_true_for_published_13_digit_examples(string eik)
    {
        Eik.IsValid(eik).ShouldBeTrue();
    }

    [Theory]
    [InlineData("000475148")] // first remainder is 10, weights 3-10 give 8
    [InlineData("001346234")] // first remainder is 10, weights 3-10 give 4
    [InlineData("007523050")] // both remainders are 10, so the check digit is 0
    [InlineData("014491770")] // both remainders are 10, so the check digit is 0
    public void IsValid_applies_fallback_weights_for_9_digit_codes(string eik)
    {
        Eik.IsValid(eik).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1750747520154")] // first remainder is 10, weights 4, 9, 5, 7 give 4
    [InlineData("1750747520207")] // first remainder is 10, weights 4, 9, 5, 7 give 7
    [InlineData("1750747520360")] // both remainders are 10, so the check digit is 0
    public void IsValid_applies_fallback_weights_for_13_digit_codes(string eik)
    {
        Eik.IsValid(eik).ShouldBeTrue();
    }

    [Theory]
    [InlineData("175074751")]
    [InlineData("176040024")]
    [InlineData("111111111")]
    [InlineData("123456789")]
    [InlineData("000475140")] // would be valid if the fallback weights were not applied
    public void IsValid_returns_false_for_wrong_9_digit_check_digit(string eik)
    {
        Eik.IsValid(eik).ShouldBeFalse();
    }

    [Theory]
    [InlineData("1760400230152")] // valid parent, wrong 13th digit
    [InlineData("1760400240153")] // 13th digit matches digits 9-12, but the parent EIK is invalid
    [InlineData("1111111111111")]
    [InlineData("1234567890123")]
    public void IsValid_returns_false_for_invalid_13_digit_codes(string eik)
    {
        Eik.IsValid(eik).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("17507475")]
    [InlineData("1750747520")]
    [InlineData("17507475201")]
    [InlineData("175074752015")]
    [InlineData("17507475201540")]
    [InlineData("17507475A")]
    [InlineData("175 074 752")]
    [InlineData("BG175074752")]
    public void IsValid_returns_false_for_malformed_input(string eik)
    {
        Eik.IsValid(eik).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_returns_false_for_null()
    {
        Eik.IsValid((string?)null).ShouldBeFalse();
    }
}
