namespace BgIdentifiers.Tests;

public class GeneratorTests
{
    [Theory]
    [InlineData(1800, 1, 1, Gender.Male)]
    [InlineData(1899, 12, 31, Gender.Female)]
    [InlineData(1900, 1, 1, Gender.Female)]
    [InlineData(1999, 12, 31, Gender.Male)]
    [InlineData(2000, 2, 29, Gender.Female)]
    [InlineData(2099, 12, 31, Gender.Male)]
    public void Egn_Generate_round_trips_through_TryParse(int year, int month, int day, Gender gender)
    {
        var birthDate = new DateOnly(year, month, day);

        var egn = Egn.Generate(birthDate, gender, new Random(42));

        Egn.TryParse(egn, out var info).ShouldBeTrue();
        info.ShouldBe(new EgnInfo(birthDate, gender));
    }

    [Fact]
    public void Egn_Generate_uses_the_century_month_offsets()
    {
        Egn.Generate(new DateOnly(1880, 12, 5), Gender.Male)[2..4].ShouldBe("32");
        Egn.Generate(new DateOnly(1980, 12, 5), Gender.Male)[2..4].ShouldBe("12");
        Egn.Generate(new DateOnly(2008, 7, 3), Gender.Male)[2..4].ShouldBe("47");
    }

    [Theory]
    [InlineData(1799, 12, 31)]
    [InlineData(2100, 1, 1)]
    public void Egn_Generate_throws_for_dates_that_cannot_be_encoded(int year, int month, int day)
    {
        var exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Egn.Generate(new DateOnly(year, month, day), Gender.Male));

        exception.ParamName.ShouldBe("birthDate");
    }

    [Fact]
    public void Egn_Generate_throws_for_undefined_gender()
    {
        var exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Egn.Generate(new DateOnly(1990, 1, 1), (Gender)42));

        exception.ParamName.ShouldBe("gender");
    }

    [Fact]
    public void Generators_are_reproducible_with_the_same_seed()
    {
        var birthDate = new DateOnly(1990, 5, 17);

        Egn.Generate(birthDate, Gender.Female, new Random(7)).ShouldBe(Egn.Generate(birthDate, Gender.Female, new Random(7)));
        Lnch.Generate(new Random(7)).ShouldBe(Lnch.Generate(new Random(7)));
        Eik.Generate(new Random(7)).ShouldBe(Eik.Generate(new Random(7)));
        Eik.GenerateBranch(new Random(7)).ShouldBe(Eik.GenerateBranch(new Random(7)));
        BgIban.Generate(new Random(7)).ShouldBe(BgIban.Generate(new Random(7)));
    }

    [Fact]
    public void Generators_work_without_an_explicit_random()
    {
        Egn.IsValid(Egn.Generate(new DateOnly(1990, 5, 17), Gender.Male)).ShouldBeTrue();
        Lnch.IsValid(Lnch.Generate()).ShouldBeTrue();
        Eik.IsValid(Eik.Generate()).ShouldBeTrue();
        Eik.IsValid(Eik.GenerateBranch()).ShouldBeTrue();
        BgIban.IsValid(BgIban.Generate()).ShouldBeTrue();
    }

    [Fact]
    public void Eik_GenerateBranch_starts_with_a_valid_parent_eik()
    {
        var branch = Eik.GenerateBranch(new Random(3));

        branch.Length.ShouldBe(Eik.BranchLength);
        Eik.IsValid(branch[..Eik.Length]).ShouldBeTrue();
    }

    [Fact]
    public void BgIban_Generate_produces_the_bulgarian_format()
    {
        var iban = BgIban.Generate(new Random(5));

        iban.Length.ShouldBe(BgIban.Length);
        iban.ShouldStartWith("BG");
        iban[4..8].ShouldAllBe(c => char.IsAsciiLetterUpper(c));
        iban[8..].ShouldAllBe(c => char.IsAsciiDigit(c));
    }
}
