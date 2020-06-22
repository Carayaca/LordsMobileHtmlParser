using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Fizzler.Systems.HtmlAgilityPack;

using Humanizer;

using LordsMobile.Core.Dto;
using LordsMobile.Core.Exceptions;
using LordsMobile.Core.ProgramOptions;

using Newtonsoft.Json;

using NLog;

namespace LordsMobile.Core
{
    /// <summary>
    /// Lords Mobile Info.
    /// </summary>
    public class Info
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        private static class Constants
        {
            public const string BaseUri = "https://lordsmobilemaps.com/en/";
        }

        private static class Rx
        {
            public static readonly Regex KingdomNumber = new Regex(@"(?<Number>\d+)-(?<Name>\w+)", RegexOptions.Compiled);

            public static readonly Regex PlayerMight = new Regex(@"might of (?<Might>\d[^\s]+)", RegexOptions.Compiled);

            public static readonly Regex PlayerKills = new Regex(@"kills of (?<Kills>\d.*)\.", RegexOptions.Compiled);
        }

        private static class Markup
        {
            public const string Table = "div.detaildedbody div.toptab";

            public const string Rows = "div.toptabrow";
        }

        private IContentResolver Resolver { get; }

        private readonly Lazy<IDictionary<int, string>> pvtKingdomsCache = new Lazy<IDictionary<int, string>>(LoadKingdomsCache);

        private IDictionary<int, string> KingdomsCache => this.pvtKingdomsCache.Value;

        private static Cache Cache { get; } = new Cache(".");

        private const string MigrationList = "migration.txt";

        private readonly IList<GuildDto> matchList = new List<GuildDto>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Info"/> class.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        public Info(IContentResolver resolver = null)
        {
            this.Resolver = resolver ?? new ContentResolver();
        }

        /// <summary>
        /// Parses the kingdoms.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task ParseKingdom(IMigrationOptions options)
        {
            // pre-load the kingdoms cache
            await CheckKingdomsCache();

            Log.Debug("Loading player {Name} …", options.Player);
            var player = await LoadPlayer(options.Player);
            Log.Debug("Player {Name} has {1:#,##0} might", player.Name, player.Might);

            var range = Range.Interpret(options.KingdomsRange);
            this.matchList.Clear();

            foreach (var number in range)
            {
                try
                {
                    await this.ParseKingdom(number, player, options);
                }
                catch (HtmlParseException e)
                {
                    Log.Error(e.Message);
                }
            }

            this.SaveMatchList();
        }

        private async Task ParseKingdom(int number, PlayerDto player, IMigrationOptions options)
        {
            Log.Debug("Checking kingdom {N} …", number);
            if (!this.KingdomsCache.ContainsKey(number))
            {
                Log.Warn("Kingdom {N} is not found.", number);
                return;
            }

            async Task Parse(string url, ICollection<PlayerDto> container)
            {
                var doc = await this.Resolver.Get(url);
                var table = doc.DocumentNode.QuerySelector(Markup.Table)
                            ?? throw new HtmlParseException($"Could not load players list for kingdom #{number}");

                foreach (var node in table.QuerySelectorAll(Markup.Rows))
                {
                    var x = node.SelectSingleNode("div[5]/text()");
                    var dto = new PlayerDto
                    {
                        Name = node.SelectSingleNode("div/a[@href]/text()").InnerText,
                        Might = x.InnerText.ToLong(),
                        GuildTag = node.SelectSingleNode("div[3]").InnerText,
                        GuildName = node.SelectSingleNode("div[4]").InnerText
                    };

                    container.Add(dto);
                }
            }

            IList<PlayerDto> players = new List<PlayerDto>();
            var baseUrl = $"{Constants.BaseUri}kingdom/{this.KingdomsCache[number]}/ranking/player/power/";

            Log.Debug("Load top 50 players list…");
            await Parse(baseUrl, players);
            await Parse($"{baseUrl}2", players);

            // find player rank
            var l = new SortedList<long, PlayerDto>(Comparer<long>.Create((x, y) => y.CompareTo(x)))
                        {
                            { player.Might, player }
                        };
            foreach (var dto in players)
            {
                if (!l.ContainsKey(dto.Might))
                {
                    l.Add(dto.Might, dto);
                }
            }

            var rank = l.IndexOfKey(player.Might) + 1; // index begins with 0
            var scrolls = Migration.Compute(rank);
            Log.Info("#{0}: Player rank is {1}, Scrolls: {2}", number, rank > 50 ? "50+" : rank.ToString(), scrolls);

            try
            {
                var m = Migration.Load(MigrationList);
                m[number] = scrolls;
                Migration.Save(MigrationList, m);
            }
            catch
            {
                // ignored
            }

            if (scrolls <= options.Threshold)
            {
                var arr = (await ParseKingdomGuilds(number))
                              .Where(g => g.Language == "Russian")
                              .ToArray();

                if (arr.Any())
                {
                    Log.Info("#{0}: Found {1}:", number, "russian guild".ToQuantity(arr.Length));
                    foreach (var guild in arr.OrderByDescending(g => g.Might))
                    {
                        Log.Info("{0} / {1}; {2}; {3}", guild.Name, guild.Tag, guild.Might.ToMetric(), "member".ToQuantity(guild.MemberCount));
                        this.matchList.Add(guild);
                    }
                }
            }
        }

        private async Task<PlayerDto> LoadPlayer(string playerName)
        {
            if (playerName == null)
                throw new ArgumentNullException(nameof(playerName));

            Cache.TryLoad<PlayerDto>(playerName, out var dto);
            if (dto?.Name != null)
            {
                Log.Debug("Loaded from cache.");
                return dto;
            }

            var url = $@"{Constants.BaseUri}player/{playerName}";
            var doc = await this.Resolver.Get(url);

            var desc = doc.DocumentNode.QuerySelector("div.playerdesc")
                ?? throw new HtmlParseException($@"Player {playerName} is not found");

            var m = Rx.PlayerMight.Match(desc.InnerText);
            if (!m.Success)
            {
                throw new HtmlParseException("Could not get the player might");
            }

            var p = new PlayerDto
                        {
                            Name = playerName,
                            Might = m.Groups["Might"].Value.ToLong()
                        };

            Cache.Update(playerName, p);
            return p;
        }

        private const string KingdomsJson = "kingdoms.json";

        private static IDictionary<int, string> LoadKingdomsCache()
        {
            try
            {
                do
                {
                    if (!File.Exists(KingdomsJson))
                    {
                        break;
                    }

                    Log.Debug("Try to loading external cache {FileName} …", KingdomsJson);
                    using (var fs = File.OpenRead(KingdomsJson))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var serializer = new JsonSerializer();
                        var dict = serializer.Deserialize<IDictionary<int, string>>(jsonTextReader);
                        if (dict?.Any() ?? false)
                        {
                            Log.Debug("Loaded {N} items.", dict.Count);
                            return dict;
                        }
                    }
                }
                while (false);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private async Task<IDictionary<int, string>> CreateKingdomsCache()
        {
            Log.Debug("Could not found kingdoms cache, donwloading…");
            IDictionary<int, string> dict = new Dictionary<int, string>();

            var doc = await this.Resolver.Get($@"{Constants.BaseUri}kingdom");
            var table = doc.DocumentNode.QuerySelector("div.mosaicview1")
                        ?? throw new HtmlParseException("Fail to parse kingdom list");

            foreach (var div in table.QuerySelectorAll("div.col-md-3.kingdom > div > div"))
            {
                var m = Rx.KingdomNumber.Match(div.InnerHtml);
                if (m.Success)
                {
                    var index = int.Parse(m.Groups["Number"].Value);
                    dict[index] = m.Groups["Name"].Value;
                }
            }

            using (var fs = File.OpenWrite(KingdomsJson))
            using (var wr = new StreamWriter(fs))
            {
                var serializer = new JsonSerializer
                                     {
                                         Culture = CultureInfo.InvariantCulture,
                                         Formatting = Formatting.Indented
                                     };
                serializer.Serialize(wr, dict);
            }

            Log.Debug("Found {N} items.", dict.Count);
            return dict;
        }

        private async Task CheckKingdomsCache()
        {
            if (!File.Exists(KingdomsJson))
            {
                await CreateKingdomsCache();
            }
        }

        private async Task<IList<GuildDto>> ParseKingdomGuilds(int kingdom)
        {
            async Task ParseInner(GuildDto dto)
            {
                var url = $"{Constants.BaseUri}alliance/{dto.Name}";
                var doc = await this.Resolver.Get(url);

                var header = doc.DocumentNode.QuerySelector("div.allidesc")
                             ?? throw new HtmlParseException($"Could not parse description for the guild '{dto.Name}'");
                var text = header.InnerHtml.ConvertToPlainText();
                dto.Imbue(text);

                var desc = doc.DocumentNode.QuerySelector("div.systemf");
                dto.Description = desc?.InnerHtml.ConvertToPlainText();

                // parse members
                var table = doc.DocumentNode.QuerySelectorAll(Markup.Table)
                        .FirstOrDefault(node => node.ParentNode.SelectSingleNode("parent::div//*[div[text()='Alliance members']]") != null)
                            ?? throw new HtmlParseException($"Could not load members list from guild #{dto.Name}");

                foreach (var node in table.QuerySelectorAll(Markup.Rows))
                {
                    var divs = node.SelectNodes("div");

                    var player = new PlayerDto
                                     {
                                         Name = divs[1].InnerText,
                                         Kingdom = kingdom,
                                         Might = divs[4].InnerText.ToLong(),
                                         Kills = divs[5].InnerText.ToLong(),
                                         GuildName = dto.Name,
                                         GuildTag = dto.Tag
                                     };

                    dto.Members.Add(player);
                }
            }

            // ReSharper disable once VariableHidesOuterVariable
            async Task Parse(string url, ICollection<GuildDto> container)
            {
                var doc = await this.Resolver.Get(url);
                var table = doc.DocumentNode.QuerySelector(Markup.Table)
                            ?? throw new HtmlParseException($"Could not load guilds list from kingdom #{kingdom}");

                foreach (var node in table.QuerySelectorAll(Markup.Rows))
                {
                    var divs = node.SelectNodes("div");

                    Cache.TryLoad<GuildDto>(divs[1].InnerText, out var dto);
                    if (string.IsNullOrEmpty(dto?.Name))
                    {
                        dto = new GuildDto
                                      {
                                          Kingdom = kingdom,
                                          Name = divs[1].InnerText,
                                          Tag = divs[2].InnerText,
                                          Might = divs[3].InnerText.ToLong(),
                                          Kills = divs[4].InnerText.ToLong()
                                      };

                        await ParseInner(dto);
                        Cache.Update(dto.Name, dto);
                    }

                    container.Add(dto);
                }
            }

            var url = $"{Constants.BaseUri}kingdom/{this.KingdomsCache[kingdom]}/ranking/alliance/power";

            Log.Debug("Parsing guilds…");
            IList<GuildDto> guilds = new List<GuildDto>();
            await Parse(url, guilds);

            return guilds;
        }

        private void SaveMatchList()
        {
            if (!this.matchList.Any())
            {
                return;
            }

            using (var sw = new StreamWriter("List.txt", false, Encoding.UTF8))
            {
                foreach (var it in this.matchList)
                {
                    sw.Write("#{0}; ", it.Kingdom);
                    sw.Write("{0} / {1}; ", it.Name, it.Tag);
                    sw.Write("{0}; ", it.Might.ToMetric());
                    sw.Write("member".ToQuantity(it.MemberCount));
                    sw.WriteLine();
                }
            }
        }
    }
}
