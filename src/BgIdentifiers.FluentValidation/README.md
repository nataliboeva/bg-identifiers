# BgIdentifiers.FluentValidation

[FluentValidation](https://docs.fluentvalidation.net/) rules for Bulgarian identifiers, built on
[BgIdentifiers](https://www.nuget.org/packages/BgIdentifiers).

```csharp
using BgIdentifiers.FluentValidation;
using FluentValidation;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Egn).NotEmpty().ValidEgn();
        RuleFor(c => c.CompanyId).ValidEik();
        RuleFor(c => c.VatNumber).ValidBgVat();
        RuleFor(c => c.Iban).ValidBgIban();
    }
}
```

| Rule | Error code |
| --- | --- |
| `ValidEgn()` | `EgnValidator` |
| `ValidLnch()` | `LnchValidator` |
| `ValidEik()` | `EikValidator` |
| `ValidBgVat()` | `BgVatValidator` |
| `ValidBgIban()` | `BgIbanValidator` |

Like FluentValidation's built-in rules, `null` passes - chain `NotEmpty()` or `NotNull()` to require a value.
Messages can be replaced with `WithMessage(...)`.

Documentation: https://github.com/nataliboeva/bg-identifiers
