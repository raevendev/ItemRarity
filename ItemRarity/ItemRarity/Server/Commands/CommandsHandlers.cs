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
    internal static TextCommandResult HandleSetRarityCommand(TextCommandCallingArgs args)
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

    internal static TextCommandResult HandleReloadConfigCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        ModCore.LoadConfig(serverApi);

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

        var rarities = new List<RarityConfig>(timeRun);
        float totalWeight;

        if (tier != null)
        {
            if (!ModCore.Config.Tiers.TryGetValue(tier, out var tierData))
                return TextCommandResult.Error($"Tier '{tier}' not found. Available Tiers: [{string.Join(", ", ModCore.Config.Tiers.Keys.OrderBy(k => k))}]");

            totalWeight = tierData.Values.Sum();

            for (int i = 0; i < timeRun; i++)
            {
                var rarity = Rarity.GetRandomRarityByTier(tier).Value;
                rarities.Add(rarity);
            }
        }
        else
        {
            totalWeight = ModCore.Config.Rarities.Sum(r => r.Value.Rarity);

            for (int i = 0; i < timeRun; i++)
            {
                var rarity = Rarity.GetRandomRarity().Value;
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
                    var match = ModCore.Config.Tiers[tier].FirstOrDefault(t => ModCore.Config.Rarities.TryGetValue(t.Key, out var rc) && rc.Name == rarityKey);
                    weight = match.Value;
                }
                else
                {
                    weight = ModCore.Config.Rarities.Values.FirstOrDefault(r => r.Name == rarityKey)?.Rarity ?? 0;
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