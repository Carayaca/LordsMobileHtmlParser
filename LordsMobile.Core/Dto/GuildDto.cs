using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace LordsMobile.Core.Dto
{
    /// <summary>
    /// The guild DTO.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{" + nameof(DebugString) + "}")]
    public class GuildDto
    {
        private static class Rx
        {
            public static readonly Regex Members = new Regex(@"This alliance has (?<Number>\d+) members", RegexOptions.Compiled);

            public static readonly Regex Language = new Regex(@"The alliance language is (?<Language>[\w|\s]+)", RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets or sets the home kingdom.
        /// </summary>
        [JsonProperty("kingdom")]
        public int Kingdom { get; set; }

        /// <summary>
        /// Gets or sets the member count.
        /// </summary>
        [JsonProperty("memberCount")]
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets the guild tag.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the guild name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the guild description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the guild might.
        /// </summary>
        [JsonProperty("might")]
        public long Might { get; set; }

        /// <summary>
        /// Gets or sets the guild kills.
        /// </summary>
        [JsonProperty("kills")]
        public long Kills { get; set; }

        /// <summary>
        /// Gets or sets the guild language.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        [JsonProperty("members")]
        public List<PlayerDto> Members { get; set; } = new List<PlayerDto>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugString
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0} / {1}", this.Name, this.Tag);
                sb.Append("; ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", this.Might.ToMetric());
                sb.Append("; ");
                sb.AppendFormat("{0}", this.Language);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Imbues the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        internal void Imbue(string text)
        {
            var m = Rx.Members.Match(text);
            if (m.Success)
            {
                this.MemberCount = int.Parse(m.Groups["Number"].Value);
            }

            m = Rx.Language.Match(text);
            if (m.Success)
            {
                this.Language = m.Groups["Language"].Value;
            }
        }
    }
}
