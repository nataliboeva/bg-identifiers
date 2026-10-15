namespace BgIdentifiers.Tests;

public class LnchTests
{
    [Theory]
    [InlineData("7111042925")] // python-stdnum example
    [InlineData("1234567893")]
    [InlineData("8462035716")]
    [InlineData("9999999999")]
    [InlineData("0000000000")] // checksum-only validation accepts all zeros
    public void IsValid_returns_true_for_valid_numbers(string lnch)
    {
        Lnch.IsValid(lnch).ShouldBeTrue();
        Lnch.IsValid(lnch.AsSpan()).ShouldBeTrue();
    }

    [Theory]
    [InlineData("7111042922")] // python-stdnum example with a wrong check digit
    [InlineData("7111042924")]
    [InlineData("7111042926")]
    [InlineData("8032056031")] // a valid EGN, but not a valid LNCh
    public void IsValid_returns_false_for_wrong_check_digit(string lnch)
    {
        Lnch.IsValid(lnch).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("711104292")]
    [InlineData("71110429250")]
    [InlineData("71110A2925")]
    [InlineData("7111 042925")]
    [InlineData(" 7111042925")]
    [InlineData("７１１１０４２９２５")] // full-width digits
    public void IsValid_returns_false_for_malformed_input(string lnch)
    {
        Lnch.IsValid(lnch).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_returns_false_for_null()
    {
        Lnch.IsValid((string?)null).ShouldBeFalse();
    }
}
