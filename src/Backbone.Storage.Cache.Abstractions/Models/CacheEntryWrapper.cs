using Backbone.Language.Core.Types.Abstractions.Models;

namespace Backbone.Storage.Cache.Abstractions.Models;

/// <summary>
/// Represents wrapper for cache entries with null value validation.
/// </summary>
/// <typeparam name="TValue">The type of the cached value.</typeparam>
public record CacheEntryWrapper<TValue>(TValue? Value) : INullableValueWrapper<TValue>
{
    /// <inheritdoc/>
    public bool HasValue => Value is not null;
}