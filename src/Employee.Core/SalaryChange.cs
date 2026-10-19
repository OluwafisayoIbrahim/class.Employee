namespace EmployeeManager.Core;

public sealed record SalaryChange(decimal PreviousMonthlySalary, decimal NewMonthlySalary, decimal Percentage)
{
    public decimal MonthlyIncrease => NewMonthlySalary - PreviousMonthlySalary;
    public decimal PreviousAnnualSalary => checked(PreviousMonthlySalary * 12m);
    public decimal NewAnnualSalary => checked(NewMonthlySalary * 12m);
}
