namespace EmployeeManager.Core;

/// <summary>An employee with a positive monthly salary, expressed in currency units.</summary>
public sealed class Employee
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public decimal MonthlySalary { get; private set; }
    public decimal AnnualSalary => checked(MonthlySalary * 12m);

    public Employee(string firstName, string lastName, decimal monthlySalary)
    {
        FirstName = ValidateName(firstName, nameof(firstName));
        LastName = ValidateName(lastName, nameof(lastName));
        MonthlySalary = ValidateSalary(monthlySalary);
    }

    public void Rename(string firstName, string lastName)
    {
        string first = ValidateName(firstName, nameof(firstName));
        string last = ValidateName(lastName, nameof(lastName));
        FirstName = first;
        LastName = last;
    }

    public void SetMonthlySalary(decimal salary) => MonthlySalary = ValidateSalary(salary);

    /// <summary>Applies a percentage to the current monthly salary; 10 means ten percent.</summary>
    public SalaryChange GiveRaise(decimal percentage)
    {
        if (percentage < 0)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Raise percentage cannot be negative.");
        decimal previous = MonthlySalary;
        decimal updated = ValidateSalary(checked(previous + previous * (percentage / 100m)));
        MonthlySalary = updated;
        return new SalaryChange(previous, updated, percentage);
    }

    private static decimal ValidateSalary(decimal salary)
    {
        decimal rounded = decimal.Round(salary, 2, MidpointRounding.AwayFromZero);
        if (rounded <= 0)
            throw new ArgumentOutOfRangeException(nameof(salary), "Monthly salary must round to at least 0.01.");
        _ = checked(rounded * 12m);
        return rounded;
    }

    private static string ValidateName(string value, string parameter)
    {
        ArgumentNullException.ThrowIfNull(value, parameter);
        string name = value.Trim();
        if (name.Length == 0 || name.Length > 100 || name.Any(char.IsControl))
            throw new ArgumentException("Names must contain 1-100 characters without control characters.", parameter);
        return name;
    }
}
