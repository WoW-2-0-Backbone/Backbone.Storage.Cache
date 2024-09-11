namespace Backbone.Storage.Cache.Abstractions.Models;

/// <summary>
/// Defines cache entry properties.
/// </summary>
public interface ICacheEntry
{
    /// <summary>
    /// Gets the cache key.
    /// </summary>
    string CacheKey { get; }
}