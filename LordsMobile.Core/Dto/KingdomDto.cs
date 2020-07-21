using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace LordsMobile.Core.Dto
{
    /// <summary>
    /// The Kingdom DTO.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class KingdomDto
    {
        private static class Rx
        {
            public static readonly Regex Numbers = new Regex(@"population of (?<Players>\d+) players.+and (?<Guilds>\d+) alliances", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets or sets the name of the kingdom.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the kingdom number.
        /// </summary>
        [JsonProperty("number")]
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the total players.
        /// </summary>
        [JsonProperty("totalPlayers")]
        public int TotalPlayers { get; set; }

        /// <summary>
        /// Gets or sets the total guilds.
        /// </summary>
        [JsonProperty("totalGuilds")]
        public int TotalGuilds { get; set; }

        /// <summary>
        /// Gets or sets the guilds.
        /// </summary>
        [JsonProperty("guilds")]
        public List<GuildDto> Guilds { get; set; } = new List<GuildDto>();

        /// <summary>
        /// Fill the statistics.
        /// </summary>
        /// <param name="text">The description text.</param>
        internal void Imbue(string text)
        {
            var m = Rx.Numbers.Match(text);
            if (m.Success)
            {
                this.TotalPlayers = int.Parse(m.Groups["Players"].Value);
                this.TotalGuilds = int.Parse(m.Groups["Guilds"].Value);
            }
        }
    }
}
