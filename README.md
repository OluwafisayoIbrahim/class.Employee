# Employee Salary Manager

A structured C#/.NET 10 console application based on the original employee class
and test program. It manages monthly salaries, derives annual salaries, and applies
percentage raises using decimal arithmetic. No third-party packages are needed.

## Run

Install the .NET 10 **SDK** (not just the runtime), then run from this folder:

```powershell
cd C:\Users\user\documents\fisayo\class.Employee
dotnet run
```

On Windows, you can also double-click **run.bat**. On Linux and macOS, use the same
`dotnet` commands from your terminal.

```powershell
dotnet run -- --demo
dotnet run -- --help
```

## Features

- Add multiple employees and view their monthly and annual salaries.
- Apply a percentage raise to a selected employee; entering `10` means 10%.
- Show before/after monthly and annual amounts and the monthly increase.
- Validate names, salary amounts, employee IDs, and raise percentages.
- Type `back` at any field to cancel without changing existing data.
- Run the original Jim Halpert and Dwight Shrute example with 10% raises.
- Exit with menu option `0` or end-of-input.

Names may contain spaces and Unicode characters. Amounts use a period for decimals;
currency symbols and thousands separators are not accepted. Salaries are in generic
currency units, with no currency conversion, taxes, or deductions. Data is held only
for the current session. Demo employees do not alter your employee list.

## Calculation rules

- Annual salary is always monthly salary multiplied by 12.
- A percentage raise applies to the current monthly salary.
- Salary values are rounded to two decimal places, with midpoints away from zero.
- A salary must round to at least 0.01, and its annual value must fit in `decimal`.
- Zero percent is allowed and leaves salary unchanged. Negative raises are rejected.
- Repeated raises compound. Invalid or overflowing changes leave the employee unchanged.

For the original example:

| Employee | Monthly before | Annual before | Monthly after 10% | Annual after |
| --- | ---: | ---: | ---: | ---: |
| Jim Halpert | 7501.00 | 90012.00 | 8251.10 | 99013.20 |
| Dwight Shrute | 7499.00 | 89988.00 | 8248.90 | 98986.80 |

## Project structure

```text
Employee.slnx                      Solution containing all three projects
Employee.csproj                    Root console project; supports dotnet run
Directory.Build.props              Shared framework and compiler settings
src/Employee.Core/
  Employee.cs                      Employee data, validation, salary calculations
  SalaryChange.cs                  Before/after raise result
src/Employee.Console/
  Program.cs                       Entry point
  CommandLine.cs                   Arguments and exit codes
  EmployeeApplication.cs           Menus, prompts, and formatting
tests/Employee.Tests/              Dependency-free automated checks
archive/
  class Employee.txt               Original class sketch
  Employee Test.txt                Original console sketch
run.bat                            Windows launcher
.github/workflows/test.yml          Windows and Linux CI
```

The core library has no console dependencies. The console receives readers and
writers so its interaction can be tested. The root project explicitly includes
only its console source files, avoiding duplicate entry points from test projects.

## Corrections to the original files

The original files are preserved unchanged under `archive/`. The implementation:

- Replaces duplicate First/FirstName and Last/LastName properties with one pair.
- Replaces stale annual salary fields and setters with a calculated read-only property.
- Applies entered percentages rather than ignoring them or treating them as flat amounts.
- Removes the second multiplication by 12.
- Replaces unchecked decimal parsing with retryable input validation.
- Completes the missing class structure and separates source files by responsibility.

## Build and test

```powershell
dotnet build Employee.slnx --configuration Release
dotnet run --project tests/Employee.Tests --configuration Release
```

The test runner executes 12 grouped checks and returns a nonzero exit code on
failure. Use this command rather than `dotnet test`; no external test framework is
required. Coverage includes the original examples, percentage raises, compounding,
rounding, validation, overflow, cancellation, employee selection, EOF, arguments,
and culture-independent decimals.

Open `Employee.slnx` in an IDE supporting .NET 10, or open this folder in your editor.

## Publish

```powershell
dotnet publish Employee.csproj --configuration Release --output artifacts/employee
dotnet artifacts/employee/Employee.dll
```

This portable output requires the .NET 10 runtime. For a standalone Windows x64
build including its runtime:

```powershell
dotnet publish Employee.csproj --configuration Release --runtime win-x64 --self-contained true --output artifacts/windows
```

Then launch `artifacts/windows/Employee.exe`. Standalone publishing may download
runtime packs. Build and publish outputs are ignored by Git.
