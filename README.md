<p align="center">
  <img src="assets/icon.png" alt="BgIdentifiers" width="96" height="96">
</p>

<h1 align="center">BgIdentifiers</h1>

<p align="center">
  Fast, allocation-free validation and parsing of Bulgarian identifiers for .NET:<br>
  <b>EGN</b>, <b>LNCh</b>, <b>EIK/BULSTAT</b>, <b>VAT numbers</b> and <b>IBANs</b>.
</p>

<p align="center">
  <a href="https://github.com/nataliboeva/bg-identifiers/actions/workflows/ci.yml"><img src="https://github.com/nataliboeva/bg-identifiers/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <a href="https://www.nuget.org/packages/BgIdentifiers"><img src="https://img.shields.io/nuget/v/BgIdentifiers?logo=nuget&label=nuget" alt="NuGet version"></a>
  <a href="https://www.nuget.org/packages/BgIdentifiers"><img src="https://img.shields.io/nuget/dt/BgIdentifiers?logo=nuget&label=downloads" alt="NuGet downloads"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue" alt="License: MIT"></a>
  <img src="https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4?logo=dotnet" alt=".NET 8.0 | 10.0">
</p>

- Allocation-free validation on `ReadOnlySpan<char>`, with convenient `string?` overloads
- Decodes the birth date and gender from an EGN, and rejects impossible dates such as 29 February 1900
- DataAnnotations attributes (`[Egn]`, `[Eik]`, ...) and FluentValidation rules (`.ValidEgn()`, ...)
- Generators for valid test data, with reproducible output from a seeded `Random`
- Nullable-annotated, documented, and no dependencies in the core package

## Installation

```shell
dotnet add package BgIdentifiers
```

| Package | Version | Contents |
| --- | --- | --- |
| [BgIdentifiers](https://www.nuget.org/packages/BgIdentifiers) | [![NuGet](https://img.shields.io/nuget/v/BgIdentifiers?label=)](https://www.nuget.org/packages/BgIdentifiers) | Validators, `Egn.TryParse`, test data generators and DataAnnotations attributes |
| [BgIdentifiers.FluentValidation](https://www.nuget.org/packages/BgIdentifiers.FluentValidation) | [![NuGet](https://img.shields.io/nuget/v/BgIdentifiers.FluentValidation?label=)](https://www.nuget.org/packages/BgIdentifiers.FluentValidation) | `ValidEgn()`, `ValidEik()` and the other FluentValidation rules |

## Quick start

```csharp
using BgIdentifiers;

Egn.IsValid("8032056031");                     // true
Lnch.IsValid("7111042925");                    // true
Eik.IsValid("175074752");                      // true  (company)
Eik.IsValid("1760400230151");                  // true  (branch)
BgVat.IsValid("BG175074752");                  // true
BgIban.IsValid("BG80BNBG96611020345678");      // true

if (Egn.TryParse("0847038794", out EgnInfo info))
{
    Console.WriteLine($"{info.BirthDate:yyyy-MM-dd} {info.Gender}");   // 2008-07-03 Female
}
```

Every `IsValid` and `TryParse` method also accepts a `ReadOnlySpan<char>`, so you can validate part of a
larger buffer without creating a substring:

```csharp
ReadOnlySpan<char> line = "EGN: 8032056031;";
bool ok = Egn.IsValid(line.Slice(5, Egn.Length));   // true
```

## Supported identifiers

| Identifier | Bulgarian name | API | Format | Checks |
| --- | --- | --- | --- | --- |
| EGN | ЕГН, Единен граждански номер | `Egn` | 10 digits | Real birth date between 1800 and 2099, weighted mod 11 check digit. `TryParse` returns the birth date and gender. |
| LNCh | ЛНЧ, Личен номер на чужденец | `Lnch` | 10 digits | Weighted mod 10 check digit |
| EIK / BULSTAT | ЕИК, Единен идентификационен код | `Eik` | 9 digits (entity) or 13 digits (branch) | Check digit 9 and, for branches, check digit 13 - including the fallback weights used when a remainder is 10 |
| VAT number | ИН по ДДС | `BgVat` | `BG` + 9 or 10 digits | `BG` + a valid 9-digit EIK, or a valid 10-digit EGN or LNCh |
| IBAN | IBAN | `BgIban` | 22 characters | `BG`, BBAN structure (`4!a4!n2!n8!c`), ISO 13616 mod 97 check digits in the range 02-98 |

### What is not validated

These are format and checksum checks only. A valid result means the identifier *could* exist, not that
it does: the library does not call the BULSTAT register, the Commercial Register, VIES or any bank.

A few edge cases are worth knowing about:

- **Inputs are not normalized.** `" 8032056031"`, `"bg175074752"` and the printed IBAN format
  `"BG80 BNBG 9661 1020 3456 78"` are all invalid. Normalize first if your users type with spaces:
  `BgIban.IsValid(input.Replace(" ", "").ToUpperInvariant())`.
- **EGN birth dates in the future are accepted.** Validation does not depend on the current date
  (see [Design decisions](#design-decisions)). Compare `info.BirthDate` with today yourself if you need to.
- **The EGN region is not decoded.** Digits 7-9 were historically allocated in ranges per region, but
  numbers were sometimes borrowed from neighbouring regions, so they are not a dependable source of
  birthplace and the library does not offer it.
- **LNCh and EIK are checksum-only.** For example, `0000000000` is a valid LNCh and `000000000` a valid EIK
  as far as the published algorithms are concerned.
- **VAT numbers issued from an NRA "official number".** Foreigners without an LNCh can be registered
  under a 10-digit number assigned by the National Revenue Agency. python-stdnum implements a check
  digit for these numbers, but it could not be confirmed against an official source, so such VAT numbers
  are currently reported as invalid. Contributions with an authoritative reference are welcome.
- **13-digit EIK codes are never VAT numbers.** Branches are VAT-registered under their parent's 9-digit EIK.

## ASP.NET Core

### DataAnnotations

The attributes are in the core package. Like the built-in attributes, they treat `null` as valid, so
combine them with `[Required]` when a value is mandatory.

```csharp
using System.ComponentModel.DataAnnotations;
using BgIdentifiers.DataAnnotations;

public class CreateCustomerRequest
{
    [Required, Egn]
    public string? Egn { get; set; }

    [Eik]
    public string? CompanyId { get; set; }

    [BgVat]
    public string? VatNumber { get; set; }

    [BgIban(ErrorMessage = "Please enter a Bulgarian IBAN without spaces.")]
    public string? Iban { get; set; }
}
```

In an `[ApiController]`, invalid requests are rejected with a `400 Bad Request` and a message such as
`The VatNumber field is not a valid Bulgarian VAT number.`

### FluentValidation

```csharp
using BgIdentifiers.FluentValidation;
using FluentValidation;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(c => c.Egn).NotEmpty().ValidEgn();
        RuleFor(c => c.CompanyId).ValidEik();
        RuleFor(c => c.VatNumber).ValidBgVat();
        RuleFor(c => c.Iban).ValidBgIban().WithMessage("Please enter a Bulgarian IBAN without spaces.");
    }
}
```

Each rule reports its own error code (`EgnValidator`, `LnchValidator`, `EikValidator`, `BgVatValidator`,
`BgIbanValidator`), which is handy for client-side localization. With a minimal API:

```csharp
app.MapPost("/customers", async (CreateCustomerRequest request, IValidator<CreateCustomerRequest> validator) =>
{
    var result = await validator.ValidateAsync(request);
    return result.IsValid ? Results.Ok() : Results.ValidationProblem(result.ToDictionary());
});
```

## Generating test data

The generators produce valid identifiers for unit tests, seed data and demos. Pass a seeded `Random`
for reproducible output.

```csharp
var random = new Random(42);

string egn = Egn.Generate(new DateOnly(1990, 5, 17), Gender.Female, random);
string lnch = Lnch.Generate(random);
string eik = Eik.Generate(random);             // 9 digits
string branch = Eik.GenerateBranch(random);    // 13 digits
string iban = BgIban.Generate(random);         // random bank code, not a real account
string vat = "BG" + Eik.Generate(random);
```

The test suite uses these generators for property-based tests with [CsCheck](https://github.com/AnthonyLloyd/CsCheck):
for example, any generated EGN must parse back to the same date and gender, and replacing any single
character of a generated IBAN after the check digits must make it invalid.

## Performance

Every `IsValid` and `TryParse` call runs in nanoseconds and allocates nothing. Results from the
[benchmarks project](bench/BgIdentifiers.Benchmarks) (BenchmarkDotNet 0.15.8, default job, .NET 10.0.8,
Windows 11, 12th Gen Intel Core i7-1255U laptop):

| Benchmark | Input | Mean | Allocated |
| --- | --- | ---: | ---: |
| `Egn.IsValid` | `8032056031` | 10.0 ns | 0 B |
| `Egn.TryParse` | `8032056031` | 9.4 ns | 0 B |
| `Lnch.IsValid` | `7111042925` | 10.5 ns | 0 B |
| `Eik.IsValid` | `175074752` | 10.5 ns | 0 B |
| `Eik.IsValid` | `1760400230151` | 15.1 ns | 0 B |
| `BgVat.IsValid` | `BG175074752` | 8.4 ns | 0 B |
| `BgIban.IsValid` | `BG80BNBG96611020345678` | 94.9 ns | 0 B |

These are single-run numbers from a laptop, so treat them as an order of magnitude rather than exact
figures. The IBAN check is slower because mod 97 has to process 22 characters one after another.
Run the benchmarks on your own machine with:

```shell
dotnet run --configuration Release --project bench/BgIdentifiers.Benchmarks -- --filter "*"
```

## Design decisions

- **Static classes, not value objects.** Most applications store these identifiers as strings and only
  need to answer "is this valid?". A `static bool IsValid(...)` is the smallest API that does that,
  and it is easy to wrap in a value object in your own domain model if you want one.
- **Spans first, strings second.** The core logic works on `ReadOnlySpan<char>`, which lets callers validate
  slices of larger buffers without allocating. The `string?` overloads are annotated with
  `[NotNullWhen(true)]`, so the compiler knows a value is not null after a successful check.
- **`TryParse` instead of exceptions.** Invalid input is an expected outcome for a validator, not an
  exceptional one, so there is no throwing `Parse` method.
- **Deterministic results.** `IsValid` never looks at the clock, so the same input always gives the
  same answer - in tests, in CI and in production. The trade-off is that future EGN birth dates are accepted.
- **Strict input.** No trimming, case folding or removal of separators. Normalization rules differ between
  applications (should `bg` be accepted? dashes?), so the caller stays in control.
- **Only claim what the algorithms support.** The EGN region is not decoded, NRA official numbers are not
  guessed, and IBAN check digits 00, 01 and 99 are rejected even though they can satisfy mod 97, because
  ISO 7064 never produces them.
- **`null` is valid in attributes and rules.** This mirrors `[EmailAddress]` and FluentValidation's built-in
  rules, so presence and format are validated separately with `[Required]` or `NotEmpty()`.
- **Verified against independent sources.** Hand-written test vectors come from the sources below, and
  the tests check all one million YYMMDD combinations against a date oracle built on `DateOnly.TryParseExact`.

## Building from source

```shell
dotnet build
dotnet test
dotnet pack --configuration Release --output artifacts
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for the repository layout and guidelines.

## Releasing

Releases are published to nuget.org by [`release.yml`](.github/workflows/release.yml) when a `v*` tag is pushed.
The version comes from the tag through [MinVer](https://github.com/adamralph/minver), so there is no version
number to edit in the project files. Publishing uses
[NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): the workflow
exchanges a GitHub OIDC token for a short-lived API key, so no long-lived NuGet API key is stored in GitHub.

### One-time setup

1. **nuget.org:** sign in, open your user menu and choose **Trusted Publishing**, then add a policy:
   - Repository Owner: `nataliboeva`
   - Repository: `bg-identifiers`
   - Workflow File: `release.yml` (file name only)
   - Environment: `release`

   If the form asks for scopes, allow publishing new packages and new versions, with the glob pattern `BgIdentifiers*`.
   For a private repository the policy is only active for 7 days until the first successful publish; a public
   repository does not have this limitation.
2. **GitHub repository secret:** under *Settings > Secrets and variables > Actions*, add `NUGET_USER` with your
   nuget.org **username** (profile name, not your email address).
3. **GitHub environment:** under *Settings > Environments*, create an environment named `release`. Optionally add
   yourself as a required reviewer so every release waits for your approval.

### Publishing a version

1. Move the `Unreleased` entries in [CHANGELOG.md](CHANGELOG.md) under a new version heading and commit.
2. Tag the commit and push the tag:

   ```shell
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. The workflow builds, tests, packs and pushes both packages and their symbol packages.

After the first release, consider setting `<PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>`
in `src/Directory.Build.props`, so that package validation also catches breaking API changes.

## Sources

- **EGN:** [Единен граждански номер](https://bg.wikipedia.org/wiki/%D0%95%D0%B4%D0%B8%D0%BD%D0%B5%D0%BD_%D0%B3%D1%80%D0%B0%D0%B6%D0%B4%D0%B0%D0%BD%D1%81%D0%BA%D0%B8_%D0%BD%D0%BE%D0%BC%D0%B5%D1%80) (Bulgarian Wikipedia) and
  [Unique citizenship number](https://en.wikipedia.org/wiki/Unique_citizenship_number) (English Wikipedia)
- **EIK / BULSTAT:** the algorithm description published by the BULSTAT register of the Registry Agency
  ([archived copy](https://web.archive.org/web/20150416055434/http://bulstat.registryagency.bg:80/About.html))
- **IBAN:** [SWIFT IBAN Registry](https://www.swift.com/standards/data-standards/iban-international-bank-account-number) and
  [International Bank Account Number](https://en.wikipedia.org/wiki/International_Bank_Account_Number) (Wikipedia)
- **VAT numbers:** [VIES](https://ec.europa.eu/taxation_customs/vies/) (European Commission)
- **Reference implementations and test vectors:**
  [python-stdnum](https://github.com/arthurdejong/python-stdnum/tree/master/stdnum/bg) (`egn`, `pnf`, `vat`) and
  [django-localflavor](https://github.com/django/django-localflavor/blob/master/localflavor/bg/validators.py) (EGN and EIK)

## License

[MIT](LICENSE) - Copyright (c) 2026 Natali Boeva
