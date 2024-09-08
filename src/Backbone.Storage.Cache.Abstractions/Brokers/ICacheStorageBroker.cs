using Backbone.Storage.Cache.Abstractions.Models;

namespace Backbone.Storage.Cache.Abstractions.Brokers;

/// <summary>
/// Defines cache storage broker functionality.
/// </summary>
public interface ICacheStorageBroker
{
    /// <summary>
    /// Gets a cache entry value with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value of the cache entry if found; otherwise, null.</returns>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to get a cache entry with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing a boolean indicating if the entry was found, and the value if found.</returns>
    ValueTask<(bool found, T? value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cache entry with the specified key. If not found, sets the cache entry using the provided value.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The value to set if the key is not found in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="onCacheHit">An action to be invoked if the key is found in the cache.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from cache if found; otherwise, the newly set value.</returns>
    ValueTask<T?> GetOrSetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cache entry with the specified key. If not found, sets the cache entry using the provided value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="valueProvider">A function that provides the value to set if the key is not found in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="onCacheHit">An action to be invoked if the key is found in the cache.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from cache if found; otherwise, the newly set value.</returns>
    ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a cache entry with the specified key. If not found, sets the cache entry using the provided value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="valueProvider">A function that provides the value to set if the key is not found in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="onCacheHit">An action to be invoked if the key is found in the cache.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from cache if found; otherwise, the newly set value.</returns>
    ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets a cache entry with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The provided value.</returns>
    ValueTask<T> SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets a cache entry with the specified key and the entry from the value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="valueProvider">A function that provides the value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from the value factory.</returns>
    ValueTask<T> SetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets a cache entry with the specified key and the entry from the value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="valueProvider">A function that provides the value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from the value factory.</returns>
    ValueTask<T> SetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes the cache entry with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default);
}