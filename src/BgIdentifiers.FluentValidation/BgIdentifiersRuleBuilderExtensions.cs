using FluentValidation;

namespace BgIdentifiers.FluentValidation;

/// <summary>
/// FluentValidation rules for Bulgarian identifiers.
/// </summary>
/// <remarks>
/// Every rule treats <see langword="null"/> as valid, like FluentValidation's built-in rules.
/// Chain <c>NotEmpty()</c> or <c>NotNull()</c> to require a value. Each rule uses its validator
/// name (for example <c>EgnValidator</c>) as the error code.
/// </remarks>
public static class BgIdentifiersRuleBuilderExtensions
{
    /// <summary>
    /// Defines a validator that fails if the string is not a valid EGN.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined.</param>
    /// <returns>The rule builder options, for chaining <c>WithMessage</c> and similar calls.</returns>
    /// <seealso cref="Egn.IsValid(string?)"/>
    public static IRuleBuilderOptions<T, string?> ValidEgn<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.SetValidator(new BgIdentifierValidator<T>(
            "EgnValidator", Egn.IsValid, "'{PropertyName}' must be a valid EGN."));

    /// <summary>
    /// Defines a validator that fails if the string is not a valid LNCh.
    /// </summary>
    /// <inheritdoc cref="ValidEgn{T}(IRuleBuilder{T, string?})" path="/typeparam|/param|/returns"/>
    /// <seealso cref="Lnch.IsValid(string?)"/>
    public static IRuleBuilderOptions<T, string?> ValidLnch<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.SetValidator(new BgIdentifierValidator<T>(
            "LnchValidator", Lnch.IsValid, "'{PropertyName}' must be a valid LNCh."));

    /// <summary>
    /// Defines a validator that fails if the string is not a valid 9-digit or 13-digit EIK/BULSTAT code.
    /// </summary>
    /// <inheritdoc cref="ValidEgn{T}(IRuleBuilder{T, string?})" path="/typeparam|/param|/returns"/>
    /// <seealso cref="Eik.IsValid(string?)"/>
    public static IRuleBuilderOptions<T, string?> ValidEik<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.SetValidator(new BgIdentifierValidator<T>(
            "EikValidator", Eik.IsValid, "'{PropertyName}' must be a valid EIK/BULSTAT code."));

    /// <summary>
    /// Defines a validator that fails if the string is not a valid Bulgarian VAT number, such as <c>BG175074752</c>.
    /// </summary>
    /// <inheritdoc cref="ValidEgn{T}(IRuleBuilder{T, string?})" path="/typeparam|/param|/returns"/>
    /// <seealso cref="BgVat.IsValid(string?)"/>
    public static IRuleBuilderOptions<T, string?> ValidBgVat<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.SetValidator(new BgIdentifierValidator<T>(
            "BgVatValidator", BgVat.IsValid, "'{PropertyName}' must be a valid Bulgarian VAT number."));

    /// <summary>
    /// Defines a validator that fails if the string is not a valid Bulgarian IBAN in electronic format.
    /// </summary>
    /// <inheritdoc cref="ValidEgn{T}(IRuleBuilder{T, string?})" path="/typeparam|/param|/returns"/>
    /// <seealso cref="BgIban.IsValid(string?)"/>
    public static IRuleBuilderOptions<T, string?> ValidBgIban<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.SetValidator(new BgIdentifierValidator<T>(
            "BgIbanValidator", BgIban.IsValid, "'{PropertyName}' must be a valid Bulgarian IBAN."));
}
