using System.Threading;
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
        /// <param name="token">The cancellation token.</param>
        /// <returns>Task{HtmlDocument}.</returns>
        Task<HtmlDocument> Get(string url, CancellationToken token);
    }
}
