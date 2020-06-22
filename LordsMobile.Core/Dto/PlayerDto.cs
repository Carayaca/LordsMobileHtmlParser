using System.Diagnostics;
using System.Globalization;
using System.Text;

using Newtonsoft.Json;

namespace LordsMobile.Core.Dto
{
    /// <summary>
    /// The player DTO.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{" + nameof(DebugString) + "}")]
    public class PlayerDto
    {
        /// <summary>
        /// Gets or sets the player name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the player might.
        /// </summary>
        [JsonProperty("might")]
        public long Might { get; set; }

        /// <summary>
        /// Gets or sets the player kills.
        /// </summary>
        [JsonProperty("kills")]
        public long Kills { get; set; }

        /// <summary>
        /// Gets or sets the guild tag.
        /// </summary>
        [JsonProperty("guildTag")]
        public string GuildTag { get; set; }

        /// <summary>
        /// Gets or sets the guild name.
        /// </summary>
        [JsonProperty("guildName")]
        public string GuildName { get; set; }

        /// <summary>
        /// Gets or sets the kingdom.
        /// </summary>
        [JsonProperty("kingdom")]
        public int Kingdom { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugString
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}", this.Name);
                sb.Append("; ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", this.Might.ToMetric());
                sb.Append("; ");
                sb.AppendFormat("{0}", this.GuildTag);
                return sb.ToString();
            }
        }
    }
}
