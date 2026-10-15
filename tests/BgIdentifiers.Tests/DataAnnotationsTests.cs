using System.ComponentModel.DataAnnotations;
using BgIdentifiers.DataAnnotations;

namespace BgIdentifiers.Tests;

public class DataAnnotationsTests
{
    [Fact]
    public void Valid_model_has_no_errors()
    {
        var model = new Applicant
        {
            PersonalNumber = "8032056031",
            ForeignerNumber = "7111042925",
            CompanyId = "175074752",
            VatNumber = "BG175074752",
            BankAccount = "BG80BNBG96611020345678",
        };

        Validate(model).ShouldBeEmpty();
    }

    [Fact]
    public void Null_values_are_valid_so_the_attributes_work_on_optional_properties()
    {
        Validate(new Applicant()).ShouldBeEmpty();
    }

    [Fact]
    public void Each_invalid_value_produces_an_error_for_its_member()
    {
        var model = new Applicant
        {
            PersonalNumber = "8032056030",
            ForeignerNumber = "7111042922",
            CompanyId = "175074751",
            VatNumber = "BG175074751",
            BankAccount = "BG81BNBG96611020345678",
        };

        var errors = Validate(model);

        errors.Select(e => e.ErrorMessage).ShouldBe(
        [
            "The PersonalNumber field is not a valid EGN.",
            "The ForeignerNumber field is not a valid LNCh.",
            "The CompanyId field is not a valid EIK/BULSTAT code.",
            "The VatNumber field is not a valid Bulgarian VAT number.",
            "The BankAccount field is not a valid Bulgarian IBAN.",
        ]);
        errors.Select(e => e.MemberNames.Single()).ShouldBe(
            ["PersonalNumber", "ForeignerNumber", "CompanyId", "VatNumber", "BankAccount"]);
    }

    [Fact]
    public void Empty_string_is_invalid()
    {
        new EgnAttribute().IsValid(string.Empty).ShouldBeFalse();
    }

    [Fact]
    public void Non_string_values_are_invalid()
    {
        new EgnAttribute().IsValid(8032056031L).ShouldBeFalse();
        new EikAttribute().IsValid(175074752).ShouldBeFalse();
    }

    [Fact]
    public void Combining_with_Required_rejects_missing_values()
    {
        var errors = Validate(new RequiredEgnModel());

        errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe("The Egn field is required.");
    }

    [Fact]
    public void Custom_error_message_is_used()
    {
        var errors = Validate(new CustomMessageModel { Egn = "1234567890" });

        errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe("Please enter a valid EGN.");
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    private sealed class Applicant
    {
        [Egn]
        public string? PersonalNumber { get; init; }

        [Lnch]
        public string? ForeignerNumber { get; init; }

        [Eik]
        public string? CompanyId { get; init; }

        [BgVat]
        public string? VatNumber { get; init; }

        [BgIban]
        public string? BankAccount { get; init; }
    }

    private sealed class RequiredEgnModel
    {
        [Required]
        [Egn]
        public string? Egn { get; init; }
    }

    private sealed class CustomMessageModel
    {
        [Egn(ErrorMessage = "Please enter a valid EGN.")]
        public string? Egn { get; init; }
    }
}
