using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpClientCache
{
    public class CachingHandler : DelegatingHandler
    {
        private static readonly TimeSpan _defaultDuration = TimeSpan.FromHours(1);

        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingHandler> _logger;
        private readonly TimeSpan _duration;

        public CachingHandler(IMemoryCache cache, ILogger<CachingHandler> logger) : this(cache, logger, _defaultDuration)
        {
        }

        public CachingHandler(IMemoryCache cache, ILogger<CachingHandler> logger, TimeSpan duration)
        {
            _cache = cache;
            _logger = logger;
            _duration = duration;
        }

        public CachingHandler(IMemoryCache cache, ILogger<CachingHandler> logger, IOptions<CachingHandlerOptions> options) : this(cache, logger)
        {
            if (options?.Value?.CacheDuration != null)
            {
                _duration = options.Value.CacheDuration.Value;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && _cache.TryGetValue(request.RequestUri!, out var response) && response is HttpResponseMessage res)
            {
                _logger.LogInformation($"Getting response from cache for {request.RequestUri}");
            }
            else
            {
                res = await base.SendAsync(request, cancellationToken);

                if (request.Method == HttpMethod.Get)
                {
                    res.EnsureSuccessStatusCode();
                    res.Content = new NoDisposeStreamContent(res.Content, cancellationToken);

                    _cache.Set(request.RequestUri!, res, DateTimeOffset.Now.Add(_duration));
                    _logger.LogInformation($"Adding response for {request.RequestUri} to cache for {_duration}");
                }
                else
                {
                    _logger.LogInformation($"Skipping cache for {request.Method} method"); 
                }
            }

            return res;
        }

    }
}
