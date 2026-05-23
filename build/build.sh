#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${CONFIGURATION:-Release}"
SKIP_TESTS="${SKIP_TESTS:-false}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
PACKAGE_PROJECT="./src/Tracing/Tracing.csproj"
TEST_PROJECT="./tests/Tracing.UnitTests/Tracing.UnitTests.csproj"
SAMPLE_PROJECT="./samples/Tracing.Samples.Console/Tracing.Samples.Console.csproj"
BENCHMARK_PROJECT="./benchmarks/Tracing.Benchmarks/Tracing.Benchmarks.csproj"

cd "$ROOT_DIR"

dotnet restore "$PACKAGE_PROJECT"
dotnet restore "$TEST_PROJECT"
dotnet restore "$SAMPLE_PROJECT"

if [ -f "$BENCHMARK_PROJECT" ]; then
  dotnet restore "$BENCHMARK_PROJECT"
fi

dotnet build "$PACKAGE_PROJECT" --configuration "$CONFIGURATION" --no-restore
dotnet build "$SAMPLE_PROJECT" --configuration "$CONFIGURATION" --no-restore /p:UseSharedCompilation=false

if [ -f "$BENCHMARK_PROJECT" ]; then
  dotnet build "$BENCHMARK_PROJECT" --configuration "$CONFIGURATION" --no-restore /p:UseSharedCompilation=false
fi

if [ "$SKIP_TESTS" != "true" ]; then
  dotnet test "$TEST_PROJECT" --configuration "$CONFIGURATION" --no-restore --logger "trx;LogFileName=test-results.trx"
fi
