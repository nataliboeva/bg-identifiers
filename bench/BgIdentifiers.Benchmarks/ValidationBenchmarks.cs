using BenchmarkDotNet.Attributes;

namespace BgIdentifiers.Benchmarks;

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private const string ValidEgn = "8032056031";
    private const string ValidLnch = "7111042925";
    private const string ValidEik = "175074752";
    private const string ValidBranchEik = "1760400230151";
    private const string ValidVat = "BG175074752";
    private const string ValidIban = "BG80BNBG96611020345678";

    [Benchmark]
    public bool EgnIsValid() => Egn.IsValid(ValidEgn);

    [Benchmark]
    public bool EgnTryParse() => Egn.TryParse(ValidEgn, out _);

    [Benchmark]
    public bool LnchIsValid() => Lnch.IsValid(ValidLnch);

    [Benchmark]
    public bool EikIsValid() => Eik.IsValid(ValidEik);

    [Benchmark]
    public bool BranchEikIsValid() => Eik.IsValid(ValidBranchEik);

    [Benchmark]
    public bool BgVatIsValid() => BgVat.IsValid(ValidVat);

    [Benchmark]
    public bool BgIbanIsValid() => BgIban.IsValid(ValidIban);
}
