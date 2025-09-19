using System;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace ItemRarity.Logging;

[PublicAPI]
public static class Logger
{
    private const string LogsPrefix = "[ItemRarity]";

    public static void Log(EnumLogType logType, string message, EnumAppSide side = EnumAppSide.Universal)
    {
        switch (side)
        {
            case EnumAppSide.Universal:
                ModCore.ServerApi?.Logger.Log(logType, $"{LogsPrefix} {message}");
                ModCore.ClientApi?.Logger.Log(logType, $"{LogsPrefix} {message}");
                break;
            case EnumAppSide.Client:
                ModCore.ClientApi?.Logger.Log(logType, $"{LogsPrefix} {message}");
                break;
            case EnumAppSide.Server:
                ModCore.ServerApi?.Logger.Log(logType, $"{LogsPrefix} {message}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }
    }

    public static void Notification(string message, EnumAppSide side = EnumAppSide.Universal)
    {
        Log(EnumLogType.Notification, message, side);
    }

    public static void Warning(string message, EnumAppSide side = EnumAppSide.Universal)
    {
        Log(EnumLogType.Warning, message, side);
    }

    public static void Error(string message, EnumAppSide side = EnumAppSide.Universal)
    {
        Log(EnumLogType.Error, message, side);
    }
}