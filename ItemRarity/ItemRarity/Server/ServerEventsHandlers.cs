using ItemRarity.Packets;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace ItemRarity.Server;

internal static class ServerEventsHandlers
{
    public static void OnPlayerJoin(IServerPlayer byplayer)
    {
        ModCore.ServerApi?.Network.GetChannel(ModCore.ConfigSyncNetChannel).SendPacket(new ServerConfigMessage
            { SerializedConfig = JsonConvert.SerializeObject(ModCore.Config) }, byplayer);
    }

    public static void OnEntitySpawn(Entity entity)
    {
        if (ModCore.Config.Tier.EnableTiers || entity is not EntityItem { Itemstack: not null, Attributes: not null } item)
            return;

        if (!RarityManager.IsSuitableFor(item.Itemstack))
            return;

        RarityManager.SetRandomRarity(item.Itemstack);
    }
}