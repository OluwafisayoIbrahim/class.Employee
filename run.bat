@echo off
cd /d "%~dp0"
dotnet run --project Employee.csproj -- %*
if errorlevel 1 goto failed
exit /b 0
:failed
echo Could not start Employee. Install the .NET 10 SDK and see README.md.
pause
exit /b 1
