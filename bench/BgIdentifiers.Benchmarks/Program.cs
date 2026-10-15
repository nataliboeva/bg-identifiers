using BenchmarkDotNet.Running;

// Run all benchmarks:   dotnet run -c Release --project bench/BgIdentifiers.Benchmarks -- --filter "*"
// Quick smoke test:     dotnet run -c Release --project bench/BgIdentifiers.Benchmarks -- --filter "*" --job dry
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
