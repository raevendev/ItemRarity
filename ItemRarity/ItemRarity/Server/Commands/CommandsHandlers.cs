using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemRarity.Config;
using ItemRarity.Models;
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
    internal static TextCommandResult HandleSetRarityCommand(TextCommandCallingArgs args)
    {
        var rarityKey = args.Parsers[0].GetValue().ToString();
        if (string.IsNullOrWhiteSpace(rarityKey))
            return TextCommandResult.Error($"Missing required parameter 'rarityKey'");
        if (!ModCore.Config.Rarity.TryGetRarity(rarityKey, out var rarity))
            return TextCommandResult.Error($"Rarity '{rarityKey}' not found");


        var activeSlot = args.Caller.Player.InventoryManager.ActiveHotbarSlot;
        var currentItemStack = activeSlot.Itemstack;
        currentItemStack.SetRarity(rarity);

        activeSlot.MarkDirty();

        return TextCommandResult.Success($"Item rarity has been set to <font color=\"{rarity.Color}\">{rarity.Name}</font>");
    }

    internal static TextCommandResult HandleReloadConfigCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        ModCore.Config = ModConfig.Load(serverApi);

        var networkChannel = serverApi.Network.GetChannel(ModCore.ConfigSyncNetChannel);
        var newConfig = new ServerConfigMessage { SerializedConfig = JsonConvert.SerializeObject(ModCore.Config) };
        networkChannel.BroadcastPacket(newConfig);

        return TextCommandResult.Success("Configuration has been reloaded");
    }

    internal static TextCommandResult HandleTestRarityCommandUnified(TextCommandCallingArgs args)
    {
        var timeRun = args.Parsers[0].GetValue() as int? ?? 0;
        var tier = args.Parsers[1].IsMissing ? null : args.Parsers[1].GetValue().ToString();

        if (timeRun <= 0)
            return TextCommandResult.Error("Run count must be greater than 0.");

        var rarities = new List<Rarity>(timeRun);
        float totalWeight;

        if (tier != null)
        {
            if (!ModCore.Config.Tier.TryGetTier(tier, out var tierData))
                return TextCommandResult.Error($"Tier '{tier}' not found. Available Tiers: [{string.Join(", ", ModCore.Config.Tier.Tiers.Keys.OrderBy(k => k))}]");

            totalWeight = tierData.Rarities.Sum(r => r.Value);

            for (var i = 0; i < timeRun; i++)
            {
                var rarity = RarityManager.GetRandomRarityByTier(tier);
                if (rarity is null)
                    continue;
                rarities.Add(rarity);
            }
        }
        else
        {
            totalWeight = ModCore.Config.Rarity.Rarities.Sum(r => r.Value.Weight);

            for (var i = 0; i < timeRun; i++)
            {
                var rarity = RarityManager.GetRandomRarity();
                if (rarity is null)
                    continue;
                rarities.Add(rarity);
            }
        }

        var results = rarities
            .GroupBy(r => r.Name)
            .Select(g =>
            {
                var rarityKey = g.Key;

                float weight;
                if (tier != null)
                {
                    var match = ModCore.Config.Tier[tier]!.Rarities.FirstOrDefault(t => ModCore.Config.Rarity.TryGetRarity(t.Key, out var rc) && rc.Name == rarityKey);
                    weight = match.Value;
                }
                else
                {
                    weight = ModCore.Config.Rarity.Rarities.Values.FirstOrDefault(r => r.Name == rarityKey)?.Weight ?? 0;
                }

                return new
                {
                    Name = rarityKey,
                    Count = g.Count(),
                    Weight = weight,
                    g.First().Color
                };
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        var message = new StringBuilder();
        message.AppendLine(tier != null
            ? $"Rarity test for tier '{tier}' ran {timeRun} time(s):"
            : $"Global rarity test ran {timeRun} time(s):");

        foreach (var r in results)
        {
            var percent = r.Weight / totalWeight * 100f;
            message.AppendLine($" - <font color=\"{r.Color}\">{r.Name}</font> : {r.Count} ({percent:F2}%)");
        }

        return TextCommandResult.Success(message.ToString());
    }

    internal static TextCommandResult HandleDebugItemAttributesCommand(TextCommandCallingArgs args)
    {
        var activeSlot = args.Caller.Player.InventoryManager.ActiveHotbarSlot;
        var currentItemStack = activeSlot.Itemstack;

        var sb = new StringBuilder();
        sb.AppendLine("Item Attributes: ");
        BuildAttributesTree(currentItemStack.Attributes, sb);
        return TextCommandResult.Success(sb.ToString());
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