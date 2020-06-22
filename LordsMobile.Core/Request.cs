using System.Net.Http;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace LordsMobile.Core
{
    /// <summary>
    /// Http request class.
    /// </summary>
    public static class Request
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:27.0) Gecko/20100101 Firefox/27.0";

        /// <summary>
        /// Get the http content.
        /// </summary>
        /// <param name="uri">
        /// The URI.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<HtmlDocument> Get(string uri)
        {
            using (var http = new HttpClient())
            {
                // User Agent
                http.DefaultRequestHeaders.UserAgent.Clear();
                http.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
                // Accept
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.ParseAdd("text/javascript, text/html, application/xml, text/xml, */*");
                // GET
                using (var message = await http.GetAsync(uri, HttpCompletionOption.ResponseContentRead))
                using (var s = await message.Content.ReadAsStreamAsync())
                {
                    var doc = new HtmlDocument
                                  {
                                      OptionFixNestedTags = true
                                  };
                    doc.Load(s);
                    return doc;
                }
            }
        }
    }
}
