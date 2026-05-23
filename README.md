# Tracing

Tracing is the repository for the Atya.Diagnostics.Tracing NuGet package.

| | |
| --- | --- |
| Repository | [https://github.com/AtyaLibraries/Tracing](https://github.com/AtyaLibraries/Tracing) |
| NuGet | Atya.Diagnostics.Tracing |
| License | MIT |

Provider-agnostic tracing helpers for .NET applications built on System.Diagnostics.Activity.

## Layout

```text
.
|-- src/Tracing/
|-- tests/Tracing.UnitTests/
|-- samples/Tracing.Samples.Console/
|-- benchmarks/Tracing.Benchmarks/
|-- build/
\-- .github/
```

## Build and test

```bash
./build/build.ps1 -Configuration Release
./build/pack.ps1 -Configuration Release
```

Artifacts land in artifacts/packages/.

## Consumer guidance

Package-specific usage guidance lives in src/Tracing/README.md.

## Public API surface

Public API changes must be tracked in src/Tracing/PublicAPI.Unshipped.txt.

