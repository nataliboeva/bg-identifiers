using BgIdentifiers.FluentValidation;
using FluentValidation;
using FluentValidation.TestHelper;

namespace BgIdentifiers.Tests.FluentValidation;

public class RuleBuilderExtensionsTests
{
    private readonly CustomerValidator _validator = new();

    [Fact]
    public void Valid_model_passes()
    {
        var customer = new Customer
        {
            PersonalNumber = "8032056031",
            ForeignerNumber = "7111042925",
            CompanyId = "1760400230151",
            VatNumber = "BG7111042925",
            BankAccount = "BG80BNBG96611020345678",
        };

        _validator.TestValidate(customer).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Null_values_pass_because_presence_is_checked_separately()
    {
        _validator.TestValidate(new Customer()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_values_fail_with_error_code_and_default_message()
    {
        var customer = new Customer
        {
            PersonalNumber = "8032056030",
            ForeignerNumber = "7111042922",
            CompanyId = "175074751",
            VatNumber = "BG175074751",
            BankAccount = "BG81BNBG96611020345678",
        };

        var result = _validator.TestValidate(customer);

        result.ShouldHaveValidationErrorFor(c => c.PersonalNumber)
            .WithErrorCode("EgnValidator")
            .WithErrorMessage("'Personal Number' must be a valid EGN.");
        result.ShouldHaveValidationErrorFor(c => c.ForeignerNumber)
            .WithErrorCode("LnchValidator")
            .WithErrorMessage("'Foreigner Number' must be a valid LNCh.");
        result.ShouldHaveValidationErrorFor(c => c.CompanyId)
            .WithErrorCode("EikValidator")
            .WithErrorMessage("'Company Id' must be a valid EIK/BULSTAT code.");
        result.ShouldHaveValidationErrorFor(c => c.VatNumber)
            .WithErrorCode("BgVatValidator")
            .WithErrorMessage("'Vat Number' must be a valid Bulgarian VAT number.");
        result.ShouldHaveValidationErrorFor(c => c.BankAccount)
            .WithErrorCode("BgIbanValidator")
            .WithErrorMessage("'Bank Account' must be a valid Bulgarian IBAN.");
    }

    [Fact]
    public void Rules_work_on_non_nullable_properties_and_can_be_combined()
    {
        var validator = new InlineValidator<Invoice>();
        validator.RuleFor(i => i.SellerVat).NotEmpty().ValidBgVat();

        validator.TestValidate(new Invoice { SellerVat = "BG175074752" }).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new Invoice { SellerVat = string.Empty })
            .ShouldHaveValidationErrorFor(i => i.SellerVat)
            .WithErrorCode("NotEmptyValidator");
        validator.TestValidate(new Invoice { SellerVat = "175074752" })
            .ShouldHaveValidationErrorFor(i => i.SellerVat)
            .WithErrorCode("BgVatValidator");
    }

    [Fact]
    public void Default_message_can_be_overridden()
    {
        var validator = new InlineValidator<Customer>();
        validator.RuleFor(c => c.PersonalNumber).ValidEgn().WithMessage("Невалидно ЕГН.");

        validator.TestValidate(new Customer { PersonalNumber = "1234567890" })
            .ShouldHaveValidationErrorFor(c => c.PersonalNumber)
            .WithErrorMessage("Невалидно ЕГН.");
    }

    private sealed class Customer
    {
        public string? PersonalNumber { get; init; }

        public string? ForeignerNumber { get; init; }

        public string? CompanyId { get; init; }

        public string? VatNumber { get; init; }

        public string? BankAccount { get; init; }
    }

    private sealed class Invoice
    {
        public string SellerVat { get; init; } = string.Empty;
    }

    private sealed class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(c => c.PersonalNumber).ValidEgn();
            RuleFor(c => c.ForeignerNumber).ValidLnch();
            RuleFor(c => c.CompanyId).ValidEik();
            RuleFor(c => c.VatNumber).ValidBgVat();
            RuleFor(c => c.BankAccount).ValidBgIban();
        }
    }
}
