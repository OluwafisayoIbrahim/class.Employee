using System.Globalization;
using EmployeeManager.Core;
using EmployeeManager.ConsoleApp;

var tests = new (string Name, Action Test)[]
{
    ("Original employees and annual salary", () => {
        Equal(90012m, new Employee("Jim", "Halpert", 7501m).AnnualSalary);
        Equal(89988m, new Employee("Dwight", "Shrute", 7499m).AnnualSalary);
    }),
    ("Ten percent raises", () => {
        var jim = new Employee("Jim", "Halpert", 7501m);
        var dwight = new Employee("Dwight", "Shrute", 7499m);
        SalaryChange change = jim.GiveRaise(10);
        dwight.GiveRaise(10);
        Equal(8251.10m, jim.MonthlySalary);
        Equal(99013.20m, jim.AnnualSalary);
        Equal(98986.80m, dwight.AnnualSalary);
        Equal(750.10m, change.MonthlyIncrease);
        Equal(90012m, change.PreviousAnnualSalary);
    }),
    ("Repeated and zero raises", () => {
        var employee = new Employee("A", "B", 1000m);
        employee.GiveRaise(10);
        employee.GiveRaise(10);
        employee.GiveRaise(0);
        Equal(1210m, employee.MonthlySalary);
    }),
    ("Money rounding", () => {
        var employee = new Employee("A", "B", 100.005m);
        Equal(100.01m, employee.MonthlySalary);
        employee.SetMonthlySalary(0.05m);
        employee.GiveRaise(10);
        Equal(0.06m, employee.MonthlySalary);
        Equal(0.72m, employee.AnnualSalary);
    }),
    ("Names and atomic rename", () => {
        var employee = new Employee(" Jim ", " Halpert ", 1);
        Check(employee.FullName == "Jim Halpert", "Trimmed name");
        Throws<ArgumentException>(() => employee.Rename("Changed", " "));
        Check(employee.FirstName == "Jim", "Partial rename");
        Throws<ArgumentException>(() => employee.Rename("A\nB", "C"));
        Throws<ArgumentNullException>(() => new Employee(null!, "B", 1));
        employee.Rename("Sam", "Jones");
        Check(employee.FullName == "Sam Jones", "Rename");
    }),
    ("Invalid salary and overflow preserve state", () => {
        var employee = new Employee("A", "B", 1000);
        foreach (decimal invalid in new[] { 0m, -1m, 0.001m })
            Throws<ArgumentOutOfRangeException>(() => employee.SetMonthlySalary(invalid));
        Throws<OverflowException>(() => employee.SetMonthlySalary(decimal.MaxValue));
        Throws<OverflowException>(() => employee.GiveRaise(decimal.MaxValue));
        Throws<ArgumentOutOfRangeException>(() => employee.GiveRaise(-10));
        Equal(1000m, employee.MonthlySalary);
        Equal(12000m, employee.AnnualSalary);
    }),
    ("Salary replacement updates annual immediately", () => {
        var employee = new Employee("A", "B", 1);
        employee.SetMonthlySalary(2000);
        Equal(24000m, employee.AnnualSalary);
    }),
    ("Interactive employee selection and raise", () => {
        string output = Run("1\nJim\nHalpert\n7501\n1\nDwight\nShrute\n7499\n3\n2\n10\n2\n0\n");
        Check(output.Contains("Dwight Shrute | Monthly: 8248.90 | Annual: 98986.80"), "Raise selected employee");
        Check(output.Contains("Jim Halpert | Monthly: 7501.00 | Annual: 90012.00"), "Other employee unchanged");
    }),
    ("Input validation and cancellation", () => {
        string output = Run("bad\n1\n\nA\nB\n0\n-1\nnope\n1000\n3\n99\n1\nback\n2\n0\n");
        Check(output.Contains("Choose a whole number"), "Menu");
        Check(output.Contains("Enter an employee ID"), "Employee ID");
        Check(output.Contains("Cancelled. No changes made."), "Cancel");
        Check(output.Contains("A B | Monthly: 1000.00 | Annual: 12000.00"), "Preserved salary");
        Check(Run("1\nA\nback\n2\n0\n").Contains("No employees yet"), "Cancelled creation");
    }),
    ("EOF at every field", () => {
        foreach (string input in new[] { "", "1\n", "1\nA\n", "1\nA\nB\n",
                     "1\nA\nB\n100\n3\n", "1\nA\nB\n100\n3\n1\n" })
            Check(Run(input).TrimEnd().EndsWith("Goodbye!"), "EOF");
    }),
    ("Demo, help and invalid arguments", () => {
        using var output = new StringWriter();
        using var error = new StringWriter();
        Check(CommandLine.Run(["--demo"], new StringReader(""), output, error) == 0, "Demo exit");
        Check(output.ToString().Contains("99013.20") && output.ToString().Contains("98986.80"), "Demo values");
        Check(CommandLine.Run(["--help"], new StringReader(""), output, error) == 0, "Help exit");
        Check(CommandLine.Run(["--bad"], new StringReader(""), output, error) == 1, "Invalid flag");
        Check(Run("4\n2\n0\n").Contains("No employees yet"), "Demo modifies session");
    }),
    ("Invariant decimal input and output", () => {
        var original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Check(Run("1\nA\nB\n1000.50\n0\n").Contains("Annual: 12006.00"), "Culture");
        } finally { CultureInfo.CurrentCulture = original; }
    })
};
int failures = 0;
foreach (var (name, test) in tests)
{
    try { test(); Console.WriteLine($"PASS {name}"); }
    catch (Exception error) { failures++; Console.Error.WriteLine($"FAIL {name}: {error.Message}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
return failures == 0 ? 0 : 1;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static void Equal(decimal expected, decimal actual) => Check(expected == actual, $"Expected {expected}, got {actual}");
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
static string Run(string text)
{
    using var input = new StringReader(text);
    using var output = new StringWriter();
    new EmployeeApplication(input, output).Run();
    return output.ToString();
}
