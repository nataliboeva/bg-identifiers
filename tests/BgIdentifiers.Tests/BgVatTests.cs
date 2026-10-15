namespace BgIdentifiers.Tests;

public class BgVatTests
{
    [Theory]
    [InlineData("BG175074752")] // python-stdnum example
    [InlineData("BG831919536")]
    [InlineData("BG000565359")]
    public void IsValid_accepts_9_digit_eik(string vat)
    {
        BgVat.IsValid(vat).ShouldBeTrue();
        BgVat.IsValid(vat.AsSpan()).ShouldBeTrue();
    }

    [Theory]
    [InlineData("BG8032056031")] // EGN, not a valid LNCh
    [InlineData("BG0847038794")] // EGN
    [InlineData("BG7111042925")] // LNCh, not a valid EGN
    [InlineData("BG8506150063")] // valid as both EGN and LNCh
    public void IsValid_accepts_10_digit_egn_or_lnch(string vat)
    {
        BgVat.IsValid(vat).ShouldBeTrue();
    }

    [Theory]
    [InlineData("BG175074751")] // wrong EIK check digit
    [InlineData("BG8032056030")] // neither a valid EGN nor a valid LNCh
    [InlineData("BG9004310000")] // EGN check digit matches, but 31 April is not a date
    public void IsValid_returns_false_for_wrong_check_digits(string vat)
    {
        BgVat.IsValid(vat).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_rejects_13_digit_branch_codes()
    {
        Eik.IsValid("1760400230151").ShouldBeTrue();

        BgVat.IsValid("BG1760400230151").ShouldBeFalse();
    }

    [Theory]
    [InlineData("175074752")] // prefix is required
    [InlineData("8032056031")]
    [InlineData("bg175074752")]
    [InlineData("Bg175074752")]
    [InlineData("GB175074752")]
    [InlineData("BG 175074752")]
    [InlineData("BG 175 074 752")]
    [InlineData(" BG175074752")]
    [InlineData("BG175074752 ")]
    [InlineData("BG")]
    [InlineData("B")]
    [InlineData("")]
    [InlineData("BG17507475")]
    [InlineData("BG17507475200")]
    [InlineData("BG17507475A")]
    public void IsValid_returns_false_for_malformed_input(string vat)
    {
        BgVat.IsValid(vat).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_returns_false_for_null()
    {
        BgVat.IsValid((string?)null).ShouldBeFalse();
    }
}
