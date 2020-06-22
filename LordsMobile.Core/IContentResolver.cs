using System.Threading.Tasks;

using HtmlAgilityPack;

namespace LordsMobile.Core
{
    /// <summary>
    /// The content resolver.
    /// </summary>
    public interface IContentResolver
    {
        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{HtmlDocument}.</returns>
        Task<HtmlDocument> Get(string url);
    }
}
