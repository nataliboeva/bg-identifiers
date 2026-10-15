# BgIdentifiers

Fast, allocation-free validation and parsing of Bulgarian identifiers for .NET 8 and .NET 10.

| Identifier | Class | What is checked |
| --- | --- | --- |
| EGN (ЕГН) | `Egn` | 10 digits, real birth date (1800-2099), check digit; decodes birth date and gender |
| LNCh (ЛНЧ) | `Lnch` | 10 digits, check digit |
| EIK / BULSTAT (ЕИК) | `Eik` | 9-digit and 13-digit (branch) codes, both check digits |
| VAT number (ИН по ДДС) | `BgVat` | `BG` + valid EIK, EGN or LNCh |
| IBAN | `BgIban` | 22 characters, BBAN structure, ISO 13616 mod 97 |

## Usage

```csharp
using BgIdentifiers;

bool ok = Egn.IsValid("8032056031");          // true

if (Egn.TryParse("0847038794", out EgnInfo info))
{
    Console.WriteLine(info.BirthDate);          // 03.07.2008
    Console.WriteLine(info.Gender);             // Female
}

Eik.IsValid("175074752");                       // true
BgVat.IsValid("BG175074752");                   // true
BgIban.IsValid("BG80BNBG96611020345678");       // true
```

Every `IsValid` method has a `ReadOnlySpan<char>` overload and does not allocate.
Inputs are not trimmed or normalized.

### DataAnnotations

```csharp
using BgIdentifiers.DataAnnotations;

public class CompanyForm
{
    [Required, Eik]
    public string? CompanyId { get; set; }

    [BgVat]
    public string? VatNumber { get; set; }
}
```

### Test data

```csharp
var egn = Egn.Generate(new DateOnly(1990, 5, 17), Gender.Female, new Random(42));
var eik = Eik.Generate();
var iban = BgIban.Generate();
```

Documentation, design notes and sources: https://github.com/nataliboeva/bg-identifiers
