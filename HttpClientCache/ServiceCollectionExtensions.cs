using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpClientCache
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddTransient<CachingHandler>();
        }

        public static IServiceCollection AddCache(this IServiceCollection services, TimeSpan duration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddTransient<CachingHandler>(sp => new CachingHandler(sp.GetRequiredService<IMemoryCache>(), sp.GetRequiredService<ILogger<CachingHandler>>(), duration));
        }

        public static IServiceCollection AddCache(this IServiceCollection services, Action<CachingHandlerOptions> options)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var opt = new CachingHandlerOptions();
            options(opt);

            return services.AddTransient<CachingHandler>(sp => new CachingHandler(sp.GetRequiredService<IMemoryCache>(), sp.GetRequiredService<ILogger<CachingHandler>>(), Options.Create(opt)));
        }
    }
}
