using System;
using McMaster.Extensions.CommandLineUtils;

namespace ThinkingHome.Migrator.CLI
{
    public class Program
    {
        private static int Invoke(string provider, string connectionString, string assembly,
            CommandOption versionOption, CommandOption timeoutOption, CommandOption listOption)
        {


            Console.WriteLine("Hello World!");
            return 0;
        }

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "migrate-database";
            app.HelpOption("-?|-h|--help");
            app.Description = "Instruct the ninja to go and attack!";

            var argProvider = app.Argument("provider", "Database provider alias or class name");
            var argConnectionString = app.Argument("connectionString", "Database connection string");
            var argAssembly = app.Argument("assembly", "Assembly which contains the set of migrations");

            var versionOption = app.Option("-v|--version <version>",
                "Target database version. Use -1",
                CommandOptionType.SingleValue);

            var timeoutOption = app.Option("-t|--timeout <timeout>",
                "SQL command timeout (in seconds).",
                CommandOptionType.SingleValue);

            var listOption = app.Option("-l|--list",
                "Show migration list instead of executing migrations.",
                CommandOptionType.SingleValue);

            app.OnExecute(() => Invoke(
                argProvider.Value,
                argConnectionString.Value,
                argAssembly.Value,
                versionOption,
                timeoutOption,
                listOption));

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.WriteLine(e.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}