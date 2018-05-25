using System;
using System.Reflection;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.CLI
{
    public class Program
    {
        private const long DEFAULT_VERSION = -1;

        private static int Invoke(string provider, string connectionString, string assemblyPath,
            CommandOption versionOption, CommandOption timeoutOption, CommandOption listOption)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                loggerFactory.AddConsole();

                var logger = loggerFactory.CreateLogger("migrate-database");

                var asm = Assembly.LoadFile(assemblyPath);

                using (var migrator = new Migrator(provider, connectionString, asm, logger))
                {
                    if (timeoutOption.HasValue())
                    {
                        migrator.Provider.CommandTimeout = Convert.ToInt32(timeoutOption.Value());
                    }

                    if (listOption.HasValue())
                    {
                        ShowList(migrator, logger);
                    }
                    else
                    {
                        Migrate(versionOption, migrator);
                    }
                }
            }

            return 0;
        }

        private static void Migrate(CommandOption versionOption, Migrator migrator)
        {
            var targetVersion = versionOption.HasValue()
                ? Convert.ToInt64(versionOption.Value())
                : DEFAULT_VERSION;

            migrator.Migrate(targetVersion);
        }

        private static void ShowList(Migrator migrator, ILogger logger)
        {
            var sb = new StringBuilder();
            var appliedMigrations = migrator.GetAppliedMigrations();

            sb.AppendLine("Available migrations:");
            foreach (var info in migrator.AvailableMigrations)
            {
                var v = info.Version;
                var marker = appliedMigrations.Contains(v) ? "✓" : " ";
                var displayVersion = v.ToString().PadLeft(3);
                var displayName = info.Type.Name.ToHumanName();

                sb.AppendLine($"{marker} {displayVersion} {displayName}");
            }

            logger.LogInformation(sb.ToString());
        }

        static void Main(string[] args)
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            var app = new CommandLineApplication();
            app.Name = "migrate-database";
            app.Description = $"ThinkingHome Database migrator - v{ver.Major}.{ver.Minor}.{ver.Revision}";
            app.HelpOption("-?|-h|--help");
            app.ExtendedHelpText = "\nSee the details on https://github.com/thinking-home/migrator#readme.";

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