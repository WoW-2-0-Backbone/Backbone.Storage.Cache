using Backbone.Storage.Cache.Abstractions.Models;

namespace Backbone.Storage.Cache.Abstractions.Settings;

/// <summary>
/// Represents the configuration settings for caching.
/// </summary>
public record CacheStorageSettings
{
    /// <summary>
    /// Gets the absolute expiration time for caching an item.
    /// </summary>
    public uint AbsoluteExpirationInSeconds { get; init; }

    /// <summary>
    /// Gets the sliding expiration time for caching an item. 
    /// </summary>
    public uint SlidingExpirationInSeconds { get; init; }

    /// <summary>
    /// Maps the cache settings to cache entry options.
    /// </summary>
    /// <returns>An instance of <see cref="CacheEntryOptions"/></returns>
    public CacheEntryOptions MapToCacheEntryOptions()
    {
        return new CacheEntryOptions(
            AbsoluteExpirationInSeconds != default ? TimeSpan.FromSeconds(AbsoluteExpirationInSeconds) : null,
            SlidingExpirationInSeconds != default ? TimeSpan.FromSeconds(SlidingExpirationInSeconds) : null);
    }
}