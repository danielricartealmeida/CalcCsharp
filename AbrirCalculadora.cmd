@echo off
cd /d "%~dp0"
dotnet run --project "Interface\Calculadora.Visual.csproj"
if errorlevel 1 pause
