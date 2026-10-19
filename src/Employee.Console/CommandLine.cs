namespace EmployeeManager.ConsoleApp;

public static class CommandLine
{
    public static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        if (args.Length > 1 || (args.Length == 1 && args[0] is not ("--demo" or "--help")))
        {
            error.WriteLine("Usage: dotnet run -- [--demo | --help]");
            return 1;
        }
        if (args.Length == 1 && args[0] == "--help")
        {
            output.WriteLine("Employee salary manager");
            output.WriteLine("Run without arguments for the menu; --demo shows the original employees with 10% raises.");
            return 0;
        }
        var application = new EmployeeApplication(input, output);
        if (args.Length == 1) application.ShowDemo();
        else application.Run();
        return 0;
    }
}
