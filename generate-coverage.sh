#!/usr/bin/env bash
set -euo pipefail

TEST_PROJECT="EnviroMonitorApp.Tests"
RESULTS_DIR="$TEST_PROJECT/TestResults"
REPORT_DIR="coverage-report"

echo "1️⃣ Running tests with coverage collector…"
dotnet test "$TEST_PROJECT" \
  --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR"

echo "2️⃣ Locating coverage file…"
COV_FILE=$(find "$RESULTS_DIR" -type f -name "coverage.cobertura.xml" | head -n1)
if [[ ! -f "$COV_FILE" ]]; then
  echo "❌ Coverage file not found at $COV_FILE" >&2
  exit 1
fi
echo "✔️ Found: $COV_FILE"

echo "3️⃣ Generating filtered HTML report…"
# Build up the args as an array so nothing gets split or executed inadvertently
ARGS=(
  "-reports:$COV_FILE"
  "-targetdir:$REPORT_DIR"
  "-reporttypes:Html"
  
"-classfilters:-EnviroMonitorApp.Views.*;-*XamlGeneratedCode*;-EnviroMonitorApp.AppShell;-EnviroMonitorApp.MainPage;-EnviroMonitorApp.MauiProgram"
)

# Now pass them all in one go:
reportgenerator "${ARGS[@]}"

echo "4️⃣ Opening coverage report…"
open "$REPORT_DIR/index.html"

