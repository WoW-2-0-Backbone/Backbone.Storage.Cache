namespace Backbone.Storage.Cache.Abstractions.Enums;

/// <summary>
/// Defines the behavior of handling nullable values.
/// </summary>public enum NullValueOnSetBehavior
public enum NullValueOnSetBehavior
{
    /// <summary>
    /// Ignores the nullable value.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Stores the nullable value.
    /// </summary>
    Store = 1,

    /// <summary>
    /// Throws an exception if a nullable value is encountered.
    /// </summary>
    Throw = 2
}