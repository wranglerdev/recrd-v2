#!/usr/bin/env pwsh
# Build local: restore -> build Debug -> testes com relatório (PRD §28).
param([string]$Solution = "recrd-agile-testing.sln")

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

dotnet restore $Solution
dotnet build $Solution --configuration Debug --no-restore

$results = "artifacts/test-results"
New-Item -ItemType Directory -Force -Path $results | Out-Null
dotnet test $Solution --no-build --logger "trx" --results-directory $results
Write-Host "Relatório de testes em $results"
