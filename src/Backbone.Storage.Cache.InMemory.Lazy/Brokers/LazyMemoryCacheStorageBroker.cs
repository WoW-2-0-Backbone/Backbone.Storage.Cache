using Backbone.Storage.Cache.Abstractions.Brokers;
using Backbone.Storage.Cache.Abstractions.Enums;
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
    /// <summary>
    /// Gets the cache storage settings.
    /// </summary>
    protected readonly CacheStorageSettings CacheSettings = cacheSettings.Value;

    /// <summary>
    /// Gets the lazy memory cache storage.
    /// </summary>
    protected readonly IAppCache LazyMemoryCacheStorage = lazyMemoryCacheStorage;

    /// <summary>
    /// Gets the memory cache entry options.
    /// </summary>
    protected readonly MemoryCacheEntryOptions MemoryCacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = cacheSettings.Value.AbsoluteExpirationInSeconds != default
            ? TimeSpan.FromSeconds(cacheSettings.Value.AbsoluteExpirationInSeconds)
            : default,
        SlidingExpiration = cacheSettings.Value.SlidingExpirationInSeconds != default
            ? TimeSpan.FromSeconds(cacheSettings.Value.SlidingExpirationInSeconds)
            : default
    };

    /// <summary>
    /// Gets a cache entry value with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The value of the cache entry if found; otherwise, null.</returns>
    public virtual async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cacheEntry = await LazyMemoryCacheStorage.GetAsync<CacheEntryWrapper<T>>(key);
        return HandleNullValueOnGet(cacheEntry);
    }

    /// <summary>
    /// Attempts to get a cache entry with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cache entry value.</typeparam>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing a boolean indicating if the entry was found, and the value if found.</returns>
    public virtual ValueTask<(bool found, T? value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<(bool, T?)>((false, default));

        var isEntryFound = LazyMemoryCacheStorage.TryGetValue<CacheEntryWrapper<T>>(key, out var cacheEntry);

        return isEntryFound && cacheEntry.HasValue
            ? new ValueTask<(bool, T?)>((true, HandleNullValueOnGet(cacheEntry)))
            : new ValueTask<(bool, T?)>((false, default));
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
    public virtual async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!LazyMemoryCacheStorage.TryGetValue(key, out CacheEntryWrapper<T>? cacheEntry))
            return await SetAsync(key, value, entryOptions, cancellationToken);

        var cachedValue = HandleNullValueOnGet(cacheEntry);
        onCacheHit?.Invoke(cachedValue!);

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
    public virtual async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!LazyMemoryCacheStorage.TryGetValue(key, out CacheEntryWrapper<T>? cacheEntry))
            return await SetAsync(key, valueProvider, entryOptions, cancellationToken);

        var cachedValue = HandleNullValueOnGet(cacheEntry);
        onCacheHit?.Invoke(cachedValue!);

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
    public virtual async ValueTask<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        Action<T>? onCacheHit = default,
        CancellationToken cancellationToken = default)
    {
        if (!LazyMemoryCacheStorage.TryGetValue(key, out CacheEntryWrapper<T>? cacheEntry))
            return await SetAsync(key, valueProvider, entryOptions, cancellationToken);

        var cachedValue = HandleNullValueOnGet(cacheEntry);
        onCacheHit?.Invoke(cachedValue!);

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
    /// <exception cref="ArgumentNullException">If storing null value is disabled and passed value is null</exception>
    public virtual ValueTask<T> SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        if (HandleNullValueOnSet(value)) return new ValueTask<T>(value);
        
        LazyMemoryCacheStorage.Add(key, new CacheEntryWrapper<T>(value), GetCacheEntryOptions(entryOptions));
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
    /// <exception cref="ArgumentNullException">If storing null value is disabled and provided value is null</exception>
    /// <returns>The value from the value factory.</returns>
    public virtual ValueTask<T> SetAsync<T>(
        string key,
        Func<T> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        var value = valueProvider();
        if (HandleNullValueOnSet(value)) return new ValueTask<T>(value);

        LazyMemoryCacheStorage.Add(key, new CacheEntryWrapper<T>(value), GetCacheEntryOptions(entryOptions));
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
    /// <exception cref="ArgumentNullException">If storing null value is disabled and provided value is null</exception>
    /// <returns>The value from the value factory.</returns>
    public async ValueTask<T> SetAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> valueProvider,
        CacheEntryOptions? entryOptions = default,
        CancellationToken cancellationToken = default)
    {
        var value = await valueProvider(cancellationToken);
        if (HandleNullValueOnSet(value)) return value;

        LazyMemoryCacheStorage.Add(key, new CacheEntryWrapper<T>(value), GetCacheEntryOptions(entryOptions));
        return value;
    }

    /// <summary>
    /// Deletes the cache entry with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public virtual ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        LazyMemoryCacheStorage.Remove(key);
        return ValueTask.CompletedTask;
    }
    
    /// <summary>
    /// Handles null value based on the cache settings.
    /// </summary>
    /// <param name="value">A cache entry value</param>
    /// <typeparam name="T">The type of cache entry value</typeparam>
    /// <exception cref="ArgumentNullException">If storing null value is disabled and provided value is null</exception>
    protected virtual bool HandleNullValueOnSet<T>(T value)
    {
        if (CacheSettings.NullValueOnSetBehavior == NullValueOnSetBehavior.Throw && value is null)
            throw new ArgumentNullException(nameof(value), "Failed to store null value in cache storage, storing null value is disabled");

        return CacheSettings.NullValueOnSetBehavior != NullValueOnSetBehavior.Ignore || value is not null;
    }
    
    /// <summary>
    /// Handles null value on get based on the cache settings.
    /// </summary>
    /// <param name="value">A cache entry value</param>
    /// <typeparam name="T">The type of cache entry value</typeparam>
    /// <returns>The value stored in cache entry.</returns>
    /// <exception cref="ArgumentNullException">If storing null value is disabled and handling on get is set to throw.</exception>
    protected virtual T? HandleNullValueOnGet<T>(CacheEntryWrapper<T>? value)
    {
        if (value is null) return default;
        
        if (CacheSettings.NullValueOnSetBehavior is NullValueOnSetBehavior.Throw or NullValueOnSetBehavior.Ignore && !value.HasValue)
            throw new ArgumentNullException(nameof(value), "Failed to get value from cache storage, null value was stored and storing null value is disabled");

        return value.Value!;
    }

    /// <summary>
    /// Gets the cache entry options based on given entry options or default options.
    /// </summary>
    /// <param name="entryOptions">Given cache entry options.</param>
    /// <returns>The memory cache entry options.</returns>
    protected virtual MemoryCacheEntryOptions GetCacheEntryOptions(CacheEntryOptions? entryOptions)
    {
        if (!entryOptions.HasValue)
            return MemoryCacheEntryOptions;

        var currentEntryOptions = MemoryCacheEntryOptions.DeepClone();

        currentEntryOptions.AbsoluteExpirationRelativeToNow = entryOptions.Value.AbsoluteExpiration;
        currentEntryOptions.SlidingExpiration = entryOptions.Value.SlidingExpiration;

        if (!entryOptions.Value.AbsoluteExpiration.HasValue && !entryOptions.Value.SlidingExpiration.HasValue)
            currentEntryOptions.Priority = CacheItemPriority.NeverRemove;

        return currentEntryOptions;
    }
}