using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

using LordsMobile.Core;
using LordsMobile.Core.ProgramOptions;

using NLog;

namespace LordsMobile
{
    /// <summary>
    /// The program class.
    /// </summary>
    public static class Program
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private enum ExitCode
        {
            Success = 0,

            Error
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task Main(string[] args)
        {
            try
            {
                var types = LoadVerbs();

                var parser = new Parser(with => with.HelpWriter = null);
                var parserResult = parser.ParseArguments(args, types);

                var r1 = await parserResult.WithParsedAsync(Run);
                parserResult.WithNotParsed(errs => DisplayHelp(r1, errs));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static async Task Run(object options)
        {
            Log.Debug("Application started.");

            switch (options)
            {
                case IMigrationOptions m:
                    await new Info().ParseKingdom(m);
                    break;
            }

            Log.Debug("Application finished.");
        }

        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null)
                .ToArray();
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            HelpText helpText;
            if (errs.IsVersion()) //check if error is version request
            {
                helpText = HelpText.AutoBuild(result);
            }
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                    {
                        h.AdditionalNewLineAfterOption = false;
                        h.AddDashesToOption = true;
                        return HelpText.DefaultParsingErrorsHandler(result, h);
                    }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
