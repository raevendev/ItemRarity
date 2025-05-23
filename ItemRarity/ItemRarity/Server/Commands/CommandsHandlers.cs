using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemRarity.Config;
using ItemRarity.Packets;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace ItemRarity.Server.Commands;

/// <summary>
/// Contains handler methods for various commands.
/// </summary>
public static class CommandsHandlers
{
    public static TextCommandResult HandleSetRarityCommand(TextCommandCallingArgs args)
    {
        var rarity = args.Parsers[0].GetValue().ToString();
        if (string.IsNullOrWhiteSpace(rarity))
        {
            return TextCommandResult.Error("Missing rarity.");
        }

        var activeSlot = args.Caller.Player.InventoryManager.ActiveHotbarSlot;
        var currentItemStack = activeSlot.Itemstack;
        var setRarity = currentItemStack.SetRarity(rarity);

        activeSlot.MarkDirty();

        return TextCommandResult.Success($"Item rarity has been set to <font color=\"{setRarity.Value.Color}\">{setRarity.Value.Name}</font>");
    }

    public static TextCommandResult HandleReloadConfigCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        ModCore.LoadConfig(serverApi);

        var networkChannel = serverApi.Network.GetChannel(ModCore.ConfigSyncNetChannel);
        var newConfig = new ServerConfigMessage { SerializedConfig = JsonConvert.SerializeObject(ModCore.Config) };
        networkChannel.BroadcastPacket(newConfig);

        return TextCommandResult.Success("Configuration has been reloaded");
    }

    public static TextCommandResult HandleTestRarityCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        var timesToRun = args.Parsers[0].GetValue().ToString();

        if (!int.TryParse(timesToRun, out var timeRun))
        {
            return TextCommandResult.Error("Missing times to run.");
        }

        var rarities = new List<RarityConfig>(timeRun);
        var totalRarity = ModCore.Config.Rarities.Sum(r => r.Value.Rarity);

        for (var i = 0; i < timeRun; i++)
        {
            var rarity = Rarity.GetRandomRarity().Value;
            rarities.Add(rarity);
        }

        var results = rarities
            .GroupBy(r => r.Name)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count(),
                g.First().Rarity,
                g.First().Color,
            })
            .OrderBy(r => r.Count)
            .ToList();

        var message = new StringBuilder();

        message.AppendLine($"Rarity test ran {timeRun} time(s):");

        foreach (var rarity in results)
        {
            var relativeChance = rarity.Rarity / totalRarity * 100f;
            message.AppendLine($" - <font color=\"{rarity.Color}\">{rarity.Name}</font> : {rarity.Count} ({relativeChance:F2}%)");
        }

        serverApi.SendMessage(args.Caller.Player, 0, message.ToString(), EnumChatType.OwnMessage);

        return TextCommandResult.Success();
    }

    public static TextCommandResult HandleDebugItemAttributesCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        var activeSlot = args.Caller.Player.InventoryManager.ActiveHotbarSlot;
        var currentItemStack = activeSlot.Itemstack;

        if (Rarity.TryGetRarityTreeAttribute(currentItemStack, out var modAttribute))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Rarity Attributes: ");
            BuildAttributesTree(modAttribute, sb, 1);
            return TextCommandResult.Success(sb.ToString());
        }

        return TextCommandResult.Success("Item has no rarity attributes.");
    }

    private static void BuildAttributesTree(ITreeAttribute attributes, StringBuilder sb, int level = 0)
    {
        foreach (var attribute in attributes)
        {
            sb.Append(new string('\t', level));
            sb.Append($"<font color=\"#1E90FF\">{attribute.Key}</font>").Append(": ");

            if (attribute.Value is ITreeAttribute treeAttribute)
            {
                sb.AppendLine();
                BuildAttributesTree(treeAttribute, sb, level + 1);
            }
            else
            {
                sb.AppendLine($"<font color=\"#32CD32\">{attribute.Value.GetValue()?.ToString() ?? "null"}</font>");
            }
        }
    }
}