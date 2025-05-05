$ErrorActionPreference = 'Stop'

# ---------- config ----------
$testProject = ".\Tests\EnviroMonitorApp.Tests\EnviroMonitorApp.Tests.csproj"
$testResults = ".\Tests\EnviroMonitorApp.Tests\TestResults"
$reportDir   = ".\CoverageReport"
# ----------------------------

Write-Host "`n» Running tests with XPlat Code Coverage..." -Foreground Cyan
dotnet restore $testProject
dotnet test $testProject `
    --configuration Debug `
    --no-build `
    --collect:"XPlat Code Coverage"

if ($LASTEXITCODE -ne 0) { throw "dotnet test failed - aborting" }

# ---------- locate newest Cobertura file ----------
$covFile = Get-ChildItem -Path $testResults -Recurse -Filter "coverage.cobertura.xml" |
           Sort-Object LastWriteTime -Descending |
           Select-Object -First 1

if (-not $covFile) { throw "No coverage.cobertura.xml found in $testResults" }

Write-Host "» Using coverage file: $($covFile.FullName)" -Foreground Cyan

# ---------- generate HTML report ----------
Remove-Item $reportDir -Recurse -Force -ErrorAction SilentlyContinue
reportgenerator `
    -reports:$($covFile.FullName) `
    -targetdir:$reportDir `
    -reporttypes:Html

# ---------- open it ----------
Start-Process "$reportDir\index.html"
