using Backbone.Storage.Cache.Abstractions.Brokers;
using Backbone.Storage.Cache.Abstractions.Models;
using Backbone.Storage.Cache.Abstractions.Settings;
using Force.DeepCloner;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Backbone.Storage.Cache.InMemory.Lazy.Brokers;

/// <summary>
/// Provides caching storage functionality using LazyCache.
/// </summary>
public class LazyMemoryCacheStorageBroker(IOptions<CacheStorageSettings> cacheSettings, IAppCache lazyMemoryCacheStorage) : ICacheStorageBroker
{
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSettings.Value.AbsoluteExpirationInSeconds),
        SlidingExpiration = TimeSpan.FromSeconds(cacheSettings.Value.SlidingExpirationInSeconds)
    };

    /// <summary>
    /// Gets a cache entry value with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value of the cache entry if found; otherwise, null.</returns>
    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await lazyMemoryCacheStorage.GetAsync<T>(key);
    }

    /// <summary>
    /// Attempts to get a cache entry with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing a boolean indicating if the entry was found, and the value if found.</returns>
    public ValueTask<(bool found, T? value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<(bool, T?)>((false, default));

        var isEntryFound = lazyMemoryCacheStorage.TryGetValue<T>(key, out var value);
        return new ValueTask<(bool, T?)>((isEntryFound, value));
    }

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
    public async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!lazyMemoryCacheStorage.TryGetValue(key, out T? cachedValue))
            return await SetAsync(key, value, entryOptions, cancellationToken);

        if (cachedValue is not null)
            onCacheHit?.Invoke(cachedValue);

        return cachedValue;
    }

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
    public async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!lazyMemoryCacheStorage.TryGetValue(key, out T? cachedValue))
            return await SetAsync(key, valueProvider, entryOptions, cancellationToken);

        if (cachedValue is not null)
            onCacheHit?.Invoke(cachedValue);

        return cachedValue;
    }

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
    public async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!lazyMemoryCacheStorage.TryGetValue(key, out T? cachedValue))
            return await SetAsync(key, valueProvider, entryOptions, cancellationToken);

        if (cachedValue is not null)
            onCacheHit?.Invoke(cachedValue);

        return cachedValue;
    }

    /// <summary>
    /// Sets a cache entry with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public ValueTask<T> SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        lazyMemoryCacheStorage.Add(key, value, GetCacheEntryOptions(entryOptions));
        return new ValueTask<T>(value);
    }

    /// <summary>
    /// Sets a cache entry with the specified key and the entry from the value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="valueProvider">A function that provides the value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from the value factory.</returns>
    public ValueTask<T> SetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        var value = valueProvider();
        lazyMemoryCacheStorage.Add(key, value, GetCacheEntryOptions(entryOptions));
        return new ValueTask<T>(value);
    }

    /// <summary>
    /// Sets a cache entry with the specified key and the entry from the value factory.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to set.</param>
    /// <param name="valueProvider">A function that provides the value to store in the cache.</param>
    /// <param name="entryOptions">Custom cache entry options. If null, default options will be used.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value from the value factory.</returns>
    public async ValueTask<T> SetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        var value = await valueProvider(cancellationToken);
        lazyMemoryCacheStorage.Add(key, value, GetCacheEntryOptions(entryOptions));
        return value;
    }

    /// <summary>
    /// Deletes the cache entry with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        lazyMemoryCacheStorage.Remove(key);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the cache entry options based on given entry options or default options.
    /// </summary>
    /// <param name="entryOptions">Given cache entry options.</param>
    /// <returns>The memory cache entry options.</returns>
    private MemoryCacheEntryOptions GetCacheEntryOptions(CacheEntryOptions? entryOptions)
    {
        if (!entryOptions.HasValue)
            return _memoryCacheEntryOptions;

        var currentEntryOptions = _memoryCacheEntryOptions.DeepClone();

        currentEntryOptions.AbsoluteExpirationRelativeToNow = entryOptions.Value.AbsoluteExpiration;
        currentEntryOptions.SlidingExpiration = entryOptions.Value.SlidingExpiration;

        if (!entryOptions.Value.AbsoluteExpiration.HasValue && !entryOptions.Value.SlidingExpiration.HasValue)
            currentEntryOptions.Priority = CacheItemPriority.NeverRemove;

        return currentEntryOptions;
    }
}