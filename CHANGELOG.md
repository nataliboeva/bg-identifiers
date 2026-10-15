# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `Egn.IsValid` and `Egn.TryParse`, which decode the birth date (1800-2099) and gender.
- `Lnch.IsValid` for personal numbers of foreigners.
- `Eik.IsValid` for 9-digit EIK/BULSTAT codes and 13-digit branch codes.
- `BgVat.IsValid` for VAT numbers (`BG` + EIK, EGN or LNCh).
- `BgIban.IsValid` for Bulgarian IBANs (structure and ISO 13616 mod 97).
- Test data generators: `Egn.Generate`, `Lnch.Generate`, `Eik.Generate`, `Eik.GenerateBranch` and `BgIban.Generate`.
- DataAnnotations attributes: `[Egn]`, `[Lnch]`, `[Eik]`, `[BgVat]` and `[BgIban]`.
- `BgIdentifiers.FluentValidation` package with `ValidEgn()`, `ValidLnch()`, `ValidEik()`, `ValidBgVat()` and `ValidBgIban()`.

[Unreleased]: https://github.com/nataliboeva/bg-identifiers/commits/main
