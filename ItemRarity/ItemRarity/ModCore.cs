using System;
using System.Linq;
using HarmonyLib;
using ItemRarity.Commands;
using ItemRarity.Config;
using ItemRarity.Extensions;
using ItemRarity.Packets;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ItemRarity;

/// <summary>
/// Mod Entry point.
/// </summary>
public sealed class ModCore : ModSystem
{
    private const string HarmonyId = "itemrarity.patches";
    private const string ConfigFileName = "itemrarity.json";
    private const string ConfigSyncNetChannel = "itemrarity.configsync";

    public static ModConfig Config = ModConfig.GetDefaultConfig();
    public static Harmony HarmonyInstance = null!;
    public static ICoreClientAPI ClientApi = null!;
    public static ICoreServerAPI ServerApi = null!;
    public static WeatherSystemServer? WeatherSystemServer;

    public override void Start(ICoreAPI api)
    {
        if (!Harmony.HasAnyPatches(HarmonyId))
        {
            HarmonyInstance = new Harmony(HarmonyId);
            HarmonyInstance.PatchAll();
        }

        LoadConfig(api);

        api.Network.RegisterChannel(ConfigSyncNetChannel).RegisterMessageType<ServerConfigMessage>();

        api.Logger.Notification("[ItemRarity] Mod loaded.");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        ClientApi = api;

        api.Network.GetChannel(ConfigSyncNetChannel).SetMessageHandler<ServerConfigMessage>(packet =>
        {
            try
            {
                var config = JsonSerializer.Deserialize<ModConfig>(packet.SerializedConfig);
                if (config != null)
                {
                    Config = config;
                    api.Logger.Notification("[ItemRarity] Received config from server.");
                }
            }
            catch (Exception e)
            {
                api.Logger.Error(e);
            }
        });
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        ServerApi = api;

        api.Event.OnEntitySpawn += OnServerEntitySpawn;
        api.Event.PlayerJoin += OnServerPlayerJoin;

        WeatherSystemServer = api.ModLoader.GetModSystem<WeatherSystemServer>();

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

    private void OnServerPlayerJoin(IServerPlayer byplayer)
    {
        ServerApi.Network.GetChannel(ConfigSyncNetChannel).SendPacket(new ServerConfigMessage
            { SerializedConfig = JsonConvert.SerializeObject(Config) }, byplayer);
    }

    private void OnServerEntitySpawn(Entity entity)
    {
        if (entity is not EntityItem item || item.Attributes == null)
            return;

        var itemStack = item.Itemstack;

        if (itemStack == null || itemStack.Item?.Tool == null || itemStack.Attributes.HasAttribute(ModAttributes.Guid))
            return;

        var rarity = GetRandomRarity();

        itemStack.SetRarity(rarity.Key);
    }
    
    public override void Dispose()
    {
        HarmonyInstance.UnpatchAll(HarmonyId);
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
            if (Config != null && Config.Rarities.Any())
            {
                api.Logger.Notification("[ItemRarity] Configuration loaded.");
                return;
            }

            Config = ModConfig.GetDefaultConfig();
            api.StoreModConfig(Config, ConfigFileName);
            api.Logger.Notification("[ItemRarity] Configuration not found. Generating default configuration.");
        }
        catch
        {
            api.Logger.Warning("[ItemRarity] Failed to load configuration. Falling back to the default configuration (Will not overwrite existing configuration).");
            Config = ModConfig.GetDefaultConfig();
        }
    }
}