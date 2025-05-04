#!/usr/bin/env bash
# 
────────────────────────────────────────────────────────────────
#  generate-coverage.sh
#  • Runs dotnet tests with coverage (Cobertura XML)
#  • Generates an HTML report in docs/coverage-report
#  • Prints the public raw.githack link for the current branch
# 
────────────────────────────────────────────────────────────────
set -euo pipefail

# --- 0) CONFIG ---------------------------------------------------
TEST_PROJECT="EnviroMonitorApp/EnviroMonitorApp.Tests"
REPORT_DIR="docs/coverage-report"
COVERAGE_DIR="coverage"
COVERAGE_XML="${COVERAGE_DIR}/coverage.cobertura.xml"

echo "▶️  Running tests & collecting coverage…"
dotnet test "$TEST_PROJECT" \
  --no-build \
  --collect:"XPlat Code Coverage" \
  /p:CollectCoverage=true \
  /p:CoverletOutput="${COVERAGE_DIR}/" \
  /p:CoverletOutputFormat=cobertura

# The collector writes to **TestResults/<guid>/coverage.cobertura.xml**.
# Grab the newest one and copy it to ${COVERAGE_XML}.
LATEST_XML=$(find "$TEST_PROJECT/TestResults" -name 'coverage.cobertura.xml' -print0 \
             | xargs -0 ls -t | head -n1)
mkdir -p "$COVERAGE_DIR"
cp "$LATEST_XML" "$COVERAGE_XML"

echo "📄 Coverage XML → ${COVERAGE_XML}"

# --- 2) Generate HTML -------------------------------------------
echo "🖨️  Generating HTML report…"

# Prefer local tool; fall back to global if available
if dotnet tool list --local | grep -q reportgenerator; then
  TOOL_CMD="dotnet tool run reportgenerator"
elif command -v reportgenerator >/dev/null 2>&1; then
  TOOL_CMD="reportgenerator"
else
  echo "❌ ReportGenerator tool not found. Install with:"
  echo "   dotnet tool install --global dotnet-reportgenerator-globaltool"
  exit 1
fi

$TOOL_CMD \
  "-reports:${COVERAGE_XML}" \
  "-targetdir:${REPORT_DIR}" \
  -reporttypes:Html > /dev/null

echo "✅ HTML report generated at ${REPORT_DIR}/index.html"

# --- 3) Build raw.githack URL -----------------------------------
#   https://raw.githack.com/<user>/<repo>/<branch>/docs/coverage-report/index.html
REPO_URL=$(git config --get remote.origin.url)

# Normalize ssh → https and strip .git suffix
REPO_URL=${REPO_URL/git@github.com:/https:\/\/github.com\/}
REPO_URL=${REPO_URL%.git}

USER_REPO=$(echo "$REPO_URL" | sed -E 's#https://github.com/([^/]+/[^/]+)#\1#')
BRANCH=$(git rev-parse --abbrev-ref HEAD)

GITHACK_URL="https://raw.githack.com/${USER_REPO}/${BRANCH}/${REPORT_DIR}/index.html"

echo
echo "🌐 Open your coverage report at:"
echo "   ${GITHACK_URL}"

