namespace ItemRarity;

/// <summary>
/// Provides logging functionality for the Item Rarity mod, sending messages to both server and client sides when available.
/// </summary>
/// <remarks>
/// This utility class simplifies logging by sending messages to both server and client APIs simultaneously,
/// ensuring that messages are properly recorded regardless of which side is active.
/// </remarks>
public static class ModLogger
{
    /// <summary>
    /// Logs an informational message to both server and client.
    /// </summary>
    /// <param name="message">The notification message to log.</param>
    /// <example>
    /// <code>
    /// ModLogger.Notification("Item rarity system initialized");
    /// </code>
    /// </example>
    public static void Notification(string message)
    {
        ModCore.ServerApi?.Logger.Notification($"[ItemRarity] {message}");
        ModCore.ClientApi?.Logger.Notification($"[ItemRarity] {message}");
    }

    /// <summary>
    /// Logs a warning message to both server and client.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    /// <example>
    /// <code>
    /// ModLogger.Warning("Unable to apply rarity effect to item");
    /// </code>
    /// </example>
    public static void Warning(string message)
    {
        ModCore.ServerApi?.Logger.Warning($"[ItemRarity] {message}");
        ModCore.ClientApi?.Logger.Warning($"[ItemRarity] {message}");
    }

    /// <summary>
    /// Logs an error message to both server and client.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <example>
    /// <code>
    /// ModLogger.Error("Failed to load rarity configuration");
    /// </code>
    /// </example>
    public static void Error(string message)
    {
        ModCore.ServerApi?.Logger.Error($"[ItemRarity] {message}");
        ModCore.ClientApi?.Logger.Error($"[ItemRarity] {message}");
    }
}