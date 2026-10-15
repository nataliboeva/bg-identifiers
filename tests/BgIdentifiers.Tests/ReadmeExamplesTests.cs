namespace BgIdentifiers.Tests;

/// <summary>
/// Keeps the code samples in README.md honest.
/// </summary>
public class ReadmeExamplesTests
{
    [Fact]
    public void Quick_start()
    {
        Egn.IsValid("8032056031").ShouldBeTrue();
        Lnch.IsValid("7111042925").ShouldBeTrue();
        Eik.IsValid("175074752").ShouldBeTrue();
        Eik.IsValid("1760400230151").ShouldBeTrue();
        BgVat.IsValid("BG175074752").ShouldBeTrue();
        BgIban.IsValid("BG80BNBG96611020345678").ShouldBeTrue();

        Egn.TryParse("0847038794", out var info).ShouldBeTrue();
        $"{info.BirthDate:yyyy-MM-dd} {info.Gender}".ShouldBe("2008-07-03 Female");
    }

    [Fact]
    public void Validating_a_slice()
    {
        ReadOnlySpan<char> line = "EGN: 8032056031;";

        Egn.IsValid(line.Slice(5, Egn.Length)).ShouldBeTrue();
    }

    [Fact]
    public void Inputs_are_not_normalized()
    {
        const string printedIban = "BG80 BNBG 9661 1020 3456 78";

        Egn.IsValid(" 8032056031").ShouldBeFalse();
        BgVat.IsValid("bg175074752").ShouldBeFalse();
        BgIban.IsValid(printedIban).ShouldBeFalse();
        BgIban.IsValid(printedIban.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant()).ShouldBeTrue();
    }

    [Fact]
    public void Checksum_only_identifiers_accept_all_zeros()
    {
        Lnch.IsValid("0000000000").ShouldBeTrue();
        Eik.IsValid("000000000").ShouldBeTrue();
    }

    [Fact]
    public void Generating_test_data()
    {
        var random = new Random(42);

        Egn.TryParse(Egn.Generate(new DateOnly(1990, 5, 17), Gender.Female, random), out var info).ShouldBeTrue();
        info.ShouldBe(new EgnInfo(new DateOnly(1990, 5, 17), Gender.Female));
        Lnch.IsValid(Lnch.Generate(random)).ShouldBeTrue();
        Eik.Generate(random).Length.ShouldBe(9);
        Eik.GenerateBranch(random).Length.ShouldBe(13);
        BgIban.IsValid(BgIban.Generate(random)).ShouldBeTrue();
        BgVat.IsValid("BG" + Eik.Generate(random)).ShouldBeTrue();
    }
}
