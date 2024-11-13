using Backbone.Storage.Cache.Abstractions.Brokers;
using Backbone.Storage.Cache.Abstractions.Settings;
using Backbone.Storage.Cache.InMemory.System.Brokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backbone.Storage.Cache.InMemory.System.DependencyInjection.Configurations;

/// <summary>
/// Provides extension methods to configure the cache storage.
/// </summary>
public static class InfraConfigurations
{
    /// <summary>
    /// Configures the cache storage to use the system in-memory cache storage.
    /// </summary>
    public static void AddInMemoryCacheStorageWithSystemInMemoryCacheStorage(this IServiceCollection services, IConfiguration configuration)
    {
        // Register settings
        services.Configure<CacheStorageSettings>(configuration.GetSection(nameof(CacheStorageSettings)));

        // Register cache storage
        services.AddSingleton<ICacheStorageBroker, SystemInMemoryCacheStorageBroker>();
    }
}