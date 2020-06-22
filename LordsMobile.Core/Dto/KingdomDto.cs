using System.Collections.Generic;

using Newtonsoft.Json;

namespace LordsMobile.Core.Dto
{
    /// <summary>
    /// The Kingdom DTO.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class KingdomDto
    {
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
        /// Gets or sets the guilds.
        /// </summary>
        [JsonProperty("guilds")]
        public List<GuildDto> Guilds { get; set; } = new List<GuildDto>();
    }
}
