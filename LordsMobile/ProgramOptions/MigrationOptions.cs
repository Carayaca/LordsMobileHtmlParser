using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using CommandLine;
using CommandLine.Text;

using LordsMobile.Core;
using LordsMobile.Core.ProgramOptions;

namespace LordsMobile.ProgramOptions
{
    /// <summary>
    /// Migration options.
    /// </summary>
    [Verb("migration", HelpText = "Migration options.")]
    // ReSharper disable once UnusedType.Global
    internal class MigrationOptions : IMigrationOptions
    {
        /// <inheritdoc />
        [Option('P', "player"
            , HelpText = "The player name."
            , Required = true
        )]
        public string Player { get; set; }

        private string kingdomsRange;

        /// <inheritdoc />
        [Option('K', "kingdom"
            , HelpText = "The kingdom number or range."
            , Required = true
        )]
        public string KingdomsRange
        {
            get => this.kingdomsRange;
            set
            {
                var arr = Range.Interpret(value);
                if (!arr.Any())
                {
                    throw new ValidationException($"Invalid range: {value}");
                }

                this.kingdomsRange = value;
            }
        }

        /// <inheritdoc />
        [Option('S', "threshold"
            , HelpText = "The maximum needed scrolls to find russian guilds."
            , Default = 1
        )]
        public int Threshold { get; set; }

        /// <summary>
        /// Gets the examples.
        /// </summary>
        [Usage]
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Check migration scrolls", new MigrationOptions
                                                                        {
                                                                            Player = "Player Name",
                                                                            KingdomsRange = "500"
                                                                        });
                yield return new Example("Check migration scrolls in range of kingdoms", new MigrationOptions
                                                                        {
                                                                            Player = "Player Name",
                                                                            KingdomsRange = "100-120,200,300-350"
                                                                        });
            }
        }
    }
}
