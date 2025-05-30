namespace ItemRarity;

public static class ModLogger
{
    public static void Notification(string message)
    {
        ModCore.ServerApi?.Logger.Notification(message);
        ModCore.ClientApi?.Logger.Notification(message);
    }
    
    public static void Warning(string message)
    {
        ModCore.ServerApi?.Logger.Warning(message);
        ModCore.ClientApi?.Logger.Warning(message);
    }

    public static void Error(string message)
    {
        ModCore.ServerApi?.Logger.Error(message);
        ModCore.ClientApi?.Logger.Error(message);
    }
}