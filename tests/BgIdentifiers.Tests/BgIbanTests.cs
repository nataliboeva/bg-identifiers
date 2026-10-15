namespace BgIdentifiers.Tests;

public class BgIbanTests
{
    [Theory]
    [InlineData("BG80BNBG96611020345678")] // example from the SWIFT IBAN registry
    [InlineData("BG18RZBB91550123456789")]
    [InlineData("BG95UNCR70001520000000")]
    [InlineData("BG75STSA93000000000001")]
    [InlineData("BG32FINV91501000000001")]
    [InlineData("BG51BPBI79401012345AB1")] // the account number part may contain letters
    [InlineData("BG02BNBG96611020000016")] // lowest possible check digits
    [InlineData("BG98BNBG96611020000034")] // highest possible check digits
    public void IsValid_returns_true_for_valid_ibans(string iban)
    {
        BgIban.IsValid(iban).ShouldBeTrue();
        BgIban.IsValid(iban.AsSpan()).ShouldBeTrue();
    }

    [Theory]
    [InlineData("BG81BNBG96611020345678")]
    [InlineData("BG80BNBG96611020345679")]
    [InlineData("BG80BNBG96611020345687")] // two characters swapped
    [InlineData("BG80BNGB96611020345678")]
    public void IsValid_returns_false_for_wrong_check_digits(string iban)
    {
        BgIban.IsValid(iban).ShouldBeFalse();
    }

    [Theory]
    // These satisfy mod 97 but can never be produced by ISO 7064 MOD 97-10.
    [InlineData("BG01BNBG96611020000034")] // same BBAN as the BG98 example above
    [InlineData("BG00BNBG96611020000052")] // same BBAN as BG97BNBG96611020000052
    public void IsValid_rejects_check_digits_outside_02_to_98(string iban)
    {
        BgIban.IsValid(iban).ShouldBeFalse();
    }

    [Theory]
    [InlineData("BG80BNB196611020345678")] // digit in the bank code
    [InlineData("BG80BNBG9661A020345678")] // letter in the branch code
    [InlineData("BG80BNBG966110A0345678")] // letter in the account type
    [InlineData("BG80BNBG9661102034567a")] // lower case in the account number
    [InlineData("BGA0BNBG96611020345678")] // letter in the check digits
    public void IsValid_returns_false_when_structure_is_wrong(string iban)
    {
        BgIban.IsValid(iban).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("BG")]
    [InlineData("BG80BNBG9661102034567")]
    [InlineData("BG80BNBG966110203456789")]
    [InlineData("bg80bnbg96611020345678")]
    [InlineData("BG80 BNBG 9661 1020 3456 78")]
    [InlineData(" BG80BNBG96611020345678")]
    [InlineData("DE89370400440532013000")] // a valid German IBAN
    [InlineData("GB82WEST12345698765432")] // a valid British IBAN
    public void IsValid_returns_false_for_malformed_or_foreign_input(string iban)
    {
        BgIban.IsValid(iban).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_returns_false_for_null()
    {
        BgIban.IsValid((string?)null).ShouldBeFalse();
    }
}
