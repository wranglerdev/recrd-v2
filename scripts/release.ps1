#!/usr/bin/env pwsh
# Release local: limpa -> restore -> testes -> publish win-x64 self-contained
# -> SHA256 -> organiza release/ (PRD §26, §27, §28).
param(
    [string]$Version = "0.1.0",
    [string]$Project = "src/Recrd.App/Recrd.App.csproj"
)

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

$publish = "artifacts/publish"
$release = "release"
Remove-Item -Recurse -Force $publish, $release -ErrorAction SilentlyContinue

dotnet restore
dotnet test recrd-agile-testing.sln --configuration Release

dotnet publish $Project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publish `
    /p:Version=$Version

New-Item -ItemType Directory -Force -Path $release | Out-Null
Copy-Item "$publish/*.exe" $release -ErrorAction SilentlyContinue

# version.json — rastreabilidade do build (PRD §30).
[ordered]@{
    version   = $Version
    gitCommit = (git rev-parse --short HEAD)
    buildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    target    = "win-x64"
} | ConvertTo-Json | Set-Content "$release/version.json"

# SHA256SUM.txt de todos os artefatos.
Get-ChildItem $release -File | ForEach-Object {
    "$((Get-FileHash $_.FullName -Algorithm SHA256).Hash)  $($_.Name)"
} | Set-Content "$release/SHA256SUM.txt"

Write-Host "Release $Version organizada em $release/"
