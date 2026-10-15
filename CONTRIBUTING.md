# Contributing

Thanks for your interest in improving BgIdentifiers! Bug reports, new test cases and fixes are all welcome.

## Getting started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and the .NET 8 runtime (the libraries and tests target both).

```shell
dotnet build
dotnet test
dotnet pack --configuration Release --output artifacts
```

Benchmarks are not part of the normal build-and-test loop:

```shell
dotnet run --configuration Release --project bench/BgIdentifiers.Benchmarks -- --filter "*"
```

## Repository layout

| Path | Contents |
| --- | --- |
| `src/BgIdentifiers` | Core package: validators, parsers, generators and DataAnnotations attributes |
| `src/BgIdentifiers.FluentValidation` | FluentValidation rule extensions |
| `tests/BgIdentifiers.Tests` | xUnit tests, including CsCheck property-based tests |
| `bench/BgIdentifiers.Benchmarks` | BenchmarkDotNet benchmarks |

## Guidelines

- **Cite a source for algorithm changes.** If you change how an identifier is validated, link an
  official or well-established reference in the pull request and add test vectors from it.
- **Add tests.** Every bug fix should come with a test that fails without the fix.
- **Keep hot paths allocation-free.** `IsValid` and `TryParse` work on `ReadOnlySpan<char>` and must not allocate.
- **Public APIs need XML documentation.** The build treats missing docs and other warnings as errors.
- **Use [Conventional Commits](https://www.conventionalcommits.org/)** for commit messages, e.g.
  `fix: reject 13-digit EIK with invalid parent`.
- Update `CHANGELOG.md` under `Unreleased` for user-visible changes.

## Reporting a wrong result

If a real identifier is rejected (or an invalid one accepted), please open an issue with a
**synthetic** example that reproduces the problem. Do not post anyone's real EGN or LNCh.
