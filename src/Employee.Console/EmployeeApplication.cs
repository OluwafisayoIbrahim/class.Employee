using System.Globalization;
using EmployeeManager.Core;

namespace EmployeeManager.ConsoleApp;

public sealed class EmployeeApplication(TextReader input, TextWriter output)
{
    private readonly List<Employee> employees = [];

    public void Run()
    {
        output.WriteLine("EMPLOYEE SALARY MANAGER");
        output.WriteLine("Amounts use currency units and a decimal point. Type back at a field to cancel.");
        while (true)
        {
            output.WriteLine("\n1 Add employee   2 View employees   3 Apply percentage raise   4 Demo   0 Exit");
            output.Write("Choose an option: ");
            string? line = input.ReadLine();
            if (line is null) break;
            if (!int.TryParse(line, out int choice) || choice < 0 || choice > 4)
            {
                output.WriteLine("Choose a whole number from 0 to 4.");
                continue;
            }
            if (choice == 0) break;
            try
            {
                switch (choice)
                {
                    case 1: AddEmployee(); break;
                    case 2: ListEmployees(); break;
                    case 3: ApplyRaise(); break;
                    case 4: ShowDemo(); break;
                }
            }
            catch (EndOfStreamException) { break; }
            catch (OperationCanceledException) { output.WriteLine("Cancelled. No changes made."); }
            catch (OverflowException) { output.WriteLine("Amount is too large. No changes made."); }
            catch (ArgumentException error) { output.WriteLine($"Error: {error.Message}"); }
        }
        output.WriteLine("Goodbye!");
    }

    public void ShowDemo()
    {
        output.WriteLine("\nOriginal example: before and after a 10% raise");
        foreach (var employee in new[] {
            new Employee("Jim", "Halpert", 7501m),
            new Employee("Dwight", "Shrute", 7499m)
        })
        {
            WriteEmployee(employee);
            WriteChange(employee, employee.GiveRaise(10));
        }
        output.WriteLine("Demo employees are not added to your session.");
    }

    private void AddEmployee()
    {
        string first = ReadField("First name");
        string last = ReadField("Last name");
        decimal salary = ReadAmount("Monthly salary", positive: true);
        var employee = new Employee(first, last, salary);
        employees.Add(employee);
        output.WriteLine($"Employee added with ID {employees.Count}.");
        WriteEmployee(employee);
    }

    private void ListEmployees()
    {
        if (employees.Count == 0) output.WriteLine("No employees yet. Choose 1 to add one.");
        for (int index = 0; index < employees.Count; index++)
        {
            output.Write($"ID {index + 1} | ");
            WriteEmployee(employees[index]);
        }
    }

    private void ApplyRaise()
    {
        ListEmployees();
        if (employees.Count == 0) return;
        int id;
        while (true)
        {
            string text = ReadField("Employee ID");
            if (int.TryParse(text, out id) && id >= 1 && id <= employees.Count) break;
            output.WriteLine("Enter an employee ID from the list.");
        }
        decimal percentage = ReadAmount("Raise percentage (10 means 10%)", positive: false);
        Employee employee = employees[id - 1];
        SalaryChange change = employee.GiveRaise(percentage);
        WriteChange(employee, change);
    }

    private string ReadField(string prompt)
    {
        while (true)
        {
            output.Write($"{prompt}: ");
            string? text = input.ReadLine();
            if (text is null) throw new EndOfStreamException();
            text = text.Trim();
            if (text.Equals("back", StringComparison.OrdinalIgnoreCase)) throw new OperationCanceledException();
            if (text.Length is > 0 and <= 100 && !text.Any(char.IsControl)) return text;
            output.WriteLine("Enter 1-100 characters without control characters, or back.");
        }
    }

    private decimal ReadAmount(string prompt, bool positive)
    {
        while (true)
        {
            string text = ReadField(prompt);
            if (decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                                 CultureInfo.InvariantCulture, out decimal value)
                && (positive ? decimal.Round(value, 2, MidpointRounding.AwayFromZero) > 0 : value >= 0))
                return value;
            output.WriteLine(positive
                ? "Enter a positive amount that rounds to at least 0.01, using a decimal point."
                : "Enter a nonnegative percentage, using a decimal point.");
        }
    }

    private void WriteEmployee(Employee employee) =>
        output.WriteLine($"{employee.FullName} | Monthly: {Money(employee.MonthlySalary)} | Annual: {Money(employee.AnnualSalary)}");

    private void WriteChange(Employee employee, SalaryChange change)
    {
        output.WriteLine($"{employee.FullName}: {change.Percentage.ToString("G", CultureInfo.InvariantCulture)}% raise");
        output.WriteLine($"Monthly: {Money(change.PreviousMonthlySalary)} -> {Money(change.NewMonthlySalary)}");
        output.WriteLine($"Annual: {Money(change.PreviousAnnualSalary)} -> {Money(change.NewAnnualSalary)}");
        output.WriteLine($"Monthly increase: {Money(change.MonthlyIncrease)}");
    }

    private static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);
}
