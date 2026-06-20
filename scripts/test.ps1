#!/usr/bin/env pwsh
# Testa a solução (PRD §28). No Windows roda tudo; em Linux passe -Filter p/ o subset.
param([string]$Filter = "recrd-agile-testing.sln")

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

dotnet restore $Filter
dotnet test $Filter --configuration Debug
