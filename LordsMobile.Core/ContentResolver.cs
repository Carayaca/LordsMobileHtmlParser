using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using HtmlAgilityPack;

using NLog;

using Polly;

namespace LordsMobile.Core
{
    /// <summary>
    /// The default content resolver.
    /// </summary>
    /// <seealso cref="IContentResolver" />
    internal class ContentResolver : IContentResolver
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private const int RetryCount = 3;

        private static IAsyncPolicy RetryPolicy { get; } = Policy
            .Handle<HttpRequestException>()
            .Or<SocketException>()
                .WaitAndRetryAsync(RetryCount,
                attempt => TimeSpan.FromSeconds(5),
                (ex, i) =>
                    {
                        Log.Warn("Request failed: {0}", ex.Message);
                        Log.Debug("Wait for {0} and retry", i);
                    });

        /// <inheritdoc />
        public async Task<HtmlDocument> Get(string url, CancellationToken token)
        {
            return await RetryPolicy.ExecuteAsync(async () => await Request.Get(url, token));
        }
    }
}
