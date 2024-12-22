using System;
using System.Linq;
using HarmonyLib;
using ItemRarity.Commands;
using ItemRarity.Config;
using ItemRarity.Extensions;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ItemRarity;

/// <summary>
/// Mod Entry point.
/// </summary>
public sealed class ModCore : ModSystem
{
    private const string HarmonyId = "itemrarity.patches";
    private const string ConfigFileName = "itemrarity.json";

    public static ICoreAPI CoreApi = null!;
    public static Harmony HarmonyInstance = null!;
    public static ModConfig Config = null!;
    public static WeatherSystemServer? WeatherSystemServer;

    public override void Start(ICoreAPI api)
    {
        CoreApi = api;
        HarmonyInstance = new Harmony(HarmonyId);
        HarmonyInstance.PatchAll();

        if (api.Side == EnumAppSide.Client)
            return;

        LoadConfig(api);

        WeatherSystemServer = api.ModLoader.GetModSystem<WeatherSystemServer>();

        api.Event.OnEntitySpawn += OnEntitySpawn;

        api.Logger.Notification("[ItemRarity] Mod loaded.");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        var parsers = api.ChatCommands.Parsers;

        var mainCommand = api.ChatCommands.Create("rarity")
            .WithDescription("Commands related to ItemRarity mod")
            .RequiresPrivilege(Privilege.root)
            .RequiresPlayer();

        mainCommand.BeginSubCommand("set")
            .WithDescription("Change the currently held item rarity.")
            .WithArgs(parsers.Word("rarity", Config.Rarities.Keys.ToArray()))
            .HandleWith(CommandsHandlers.HandleSetRarityCommand)
            .EndSubCommand();

        mainCommand.BeginSubCommand("reload")
            .WithDescription("Reload the configuration")
            .HandleWith(del => CommandsHandlers.HandleReloadConfigCommand(api, del))
            .EndSubCommand();

        mainCommand.BeginSubCommand("test")
            .WithDescription("Run the random rarity generator.")
            .WithArgs(parsers.Int("times"))
            .HandleWith(del => CommandsHandlers.HandleTestRarityCommand(api, del))
            .EndSubCommand();
    }

    private void OnEntitySpawn(Entity entity)
    {
        if (entity is not EntityItem item || item.Attributes == null)
            return;

        var itemStack = item.Itemstack;

        if (itemStack == null || itemStack.Item?.Tool == null || itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var rarity = GetRandomRarity();

        itemStack.SetRarity(rarity.Key);
    }

    /// <summary>
    /// Returns a random rarity based on the configured rarities and their associated weights.
    /// </summary>
    /// <returns>
    /// A tuple containing the key (rarity name) and value (<see cref="ItemRarityConfig"/>) of the randomly selected rarity.
    /// </returns>
    public static ItemRarityInfos GetRandomRarity()
    {
        var totalWeight = Config.Rarities.Values.Sum(i => i.Rarity);
        var randomValue = Random.Shared.NextDouble() * totalWeight;
        var cumulativeWeight = 0f;

        foreach (var item in Config.Rarities)
        {
            cumulativeWeight += item.Value.Rarity;
            if (randomValue < cumulativeWeight)
                return (item.Key, item.Value);
        }

        var first = Config.Rarities.First();
        return (first.Key, first.Value);
    }

    /// <summary>
    /// Loads the configuration for the mod from the configuration file or generates a default configuration if none is found or if an error occurs.
    /// </summary>
    /// <param name="api">The core API instance used to load the configuration.</param>
    public static void LoadConfig(ICoreAPI api)
    {
        try
        {
            Config = api.LoadModConfig<ModConfig>(ConfigFileName);
            if (Config != null)
            {
                api.Logger.Notification("[ItemRarity] Configuration loaded.");
                return;
            }

            Config = new ModConfig();
            api.Logger.Notification("[ItemRarity] Configuration not found. Generating default configuration.");
            api.StoreModConfig(Config, ConfigFileName);
        }
        catch
        {
            api.Logger.Warning("[ItemRarity] Failed to load configuration. Falling back to the default configuration.");
            Config = new ModConfig();
            api.StoreModConfig(Config, ConfigFileName);
        }
    }
}