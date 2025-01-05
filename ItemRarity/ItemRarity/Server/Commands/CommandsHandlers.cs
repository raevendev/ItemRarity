using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemRarity.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace ItemRarity.Server.Commands;

/// <summary>
/// Contains handler methods for various commands.
/// </summary>
public static class CommandsHandlers
{
    /// <summary>
    /// Handles the "/rarity set" command to change the rarity of the item in the player's active hotbar slot.
    /// </summary>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>A result indicating whether the command was successful or if there was an error.</returns>
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

    /// <summary>
    /// Handles the "/rarity reload" command to reload the configuration.
    /// </summary>
    /// <param name="serverApi">The server API instance.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>A result indicating the success or failure of the reload operation.</returns>
    public static TextCommandResult HandleReloadConfigCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        ModCore.LoadConfig(serverApi);

        return TextCommandResult.Success("Configuration has been reloaded");
    }

    /// <summary>
    /// Handles the "/rarity test" command to test the distribution of item rarities over a specified number of runs.
    /// </summary>
    /// <param name="serverApi">The server API instance.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>A result indicating whether the command was successful.</returns>
    public static TextCommandResult HandleTestRarityCommand(ICoreServerAPI serverApi, TextCommandCallingArgs args)
    {
        var timesToRun = args.Parsers[0].GetValue().ToString();

        if (!int.TryParse(timesToRun, out var timeRun))
        {
            return TextCommandResult.Error("Missing times to run.");
        }

        var rarities = new List<ItemRarityConfig>(timeRun);
        var totalRarity = ModCore.Config.Rarities.Sum(r => r.Value.Rarity);

        for (var i = 0; i < timeRun; i++)
        {
            var rarity = ModRarity.GetRandomRarity().Value;
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

        if (ModRarity.TryGetRarityTreeAttribute(currentItemStack, out var modAttribute))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Rarity Attributes: ");
            BuildAttributesTree(modAttribute, sb, 1);
            return TextCommandResult.Success(sb.ToString());
        }

        return TextCommandResult.Success("Configuration has been reloaded");
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