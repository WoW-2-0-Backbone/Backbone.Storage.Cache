namespace Backbone.Storage.Cache.Abstractions.Enums;

/// <summary>
/// Defines the behavior of handling when cache entry does not meet null value set behavior.
/// </summary>
public enum IncorrectNullValueOnGetBehavior
{
    /// <summary>
    /// Refers to ignoring the null value and returning false even if cache hit.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Refers to removing cache entry containing null value.
    /// </summary>
    Remove = 1,

    /// <summary>
    /// Refers to throwing an exception if a null value was stored.
    /// </summary>
    Throw = 2
}