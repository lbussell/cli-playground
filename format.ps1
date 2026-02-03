#!/usr/bin/env pwsh
Write-Host "Running dotnet format..." -ForegroundColor Cyan
dotnet format

Write-Host "Running CSharpier..." -ForegroundColor Cyan
dotnet csharpier format .

Write-Host "Formatting complete!" -ForegroundColor Green
