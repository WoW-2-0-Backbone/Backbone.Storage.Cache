using Backbone.Storage.Cache.Abstractions.Brokers;
using Backbone.Storage.Cache.Abstractions.Settings;
using Backbone.Storage.Cache.InMemory.Lazy.Brokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backbone.Storage.Cache.InMemory.Lazy.Configurations;

/// <summary>
/// Provides extension methods to configure the cache storage.
/// </summary>
public static class InfraConfigurations
{
    /// <summary>
    /// Configures the cache storage to use the lazy in-memory cache storage.
    /// </summary>
    public static void AddInMemoryCacheStorageWithLazyInMemoryCacheStorage(this IServiceCollection services, IConfiguration configuration)
    {
        // Register settings
        services.Configure<CacheStorageSettings>(configuration.GetSection(nameof(CacheStorageSettings)));

        // Register cache storage
        services.AddSingleton<ICacheStorageBroker, LazyMemoryCacheStorageBroker>();
    }
}