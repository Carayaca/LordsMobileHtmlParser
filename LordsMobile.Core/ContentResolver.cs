using System.Threading.Tasks;

using HtmlAgilityPack;

namespace LordsMobile.Core
{
    /// <summary>
    /// The default content resolver.
    /// </summary>
    /// <seealso cref="IContentResolver" />
    internal class ContentResolver : IContentResolver
    {
        /// <inheritdoc />
        public Task<HtmlDocument> Get(string url)
        {
            return Request.Get(url);
        }
    }
}
