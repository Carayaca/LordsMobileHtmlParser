using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

using LordsMobile.Core.Dto;

using Newtonsoft.Json;

namespace LordsMobile.Core
{
    /// <summary>
    /// The cache.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
    public class Cache
    {
        private DirectoryInfo BaseDir { get; }

        /// <summary>
        /// Expiration period in cache.
        /// </summary>
        private static readonly TimeSpan Expire = TimeSpan.FromHours(24);

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="dir">The cache directory.</param>
        public Cache(string dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }

            Directory.CreateDirectory(dir);
            this.BaseDir = new DirectoryInfo(dir);

            if (!this.BaseDir.Exists)
            {
                throw new DirectoryNotFoundException(dir);
            }
        }

        private static IDictionary<Type, string> TypeLocation { get; } = new Dictionary<Type, string>
            {
                { typeof(PlayerDto), "cache/players/" },
                { typeof(GuildDto), "cache/guilds/" }
            };


        /// <summary>
        /// Try to load object from cache.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="name">The object name.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public bool TryLoad<T>(string name, out T obj)
            where T : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!TypeLocation.ContainsKey(typeof(T)))
                throw new ApplicationException($@"Unknown object type: {typeof(T).FullName}");

            obj = null;

            var dir = Path.Combine(this.BaseDir.FullName, TypeLocation[typeof(T)]);
            Directory.CreateDirectory(dir);

            var fileName = Path.Combine(dir, name);
            fileName = Path.ChangeExtension(fileName, ".json");

            var fi = new FileInfo(fileName);

            if (!fi.Exists)
            {
                return false;
            }

            var dT = DateTime.Now - fi.LastWriteTime;
            if (dT > Expire)
            {
                return false;
            }

            try
            {
                using (var fs = File.OpenRead(fileName))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    obj = serializer.Deserialize<T>(jsonTextReader);
                }
            }
            catch
            {
                // ignored
            }

            return obj != null;
        }

        /// <summary>
        /// Update the object in cache.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="name">The object name.</param>
        /// <param name="obj">The object.</param>
        public void Update<T>(string name, T obj)
            where T : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!TypeLocation.ContainsKey(typeof(T)))
                throw new ApplicationException($@"Unknown object type: {typeof(T).FullName}");

            var dir = Path.Combine(this.BaseDir.FullName, TypeLocation[typeof(T)]);
            Directory.CreateDirectory(dir);

            var fileName = Path.Combine(dir, name);
            fileName = Path.ChangeExtension(fileName, ".json");

            using (var fs = File.OpenWrite(fileName))
            using (var wr = new StreamWriter(fs, Encoding.UTF8))
            {
                var serializer = new JsonSerializer
                                     {
                                         Culture = CultureInfo.InvariantCulture,
                                         Formatting = Formatting.Indented
                                     };
                serializer.Serialize(wr, obj);
            }
        }
    }
}
