using System;
using System.Linq;
using HarmonyLib;
using ItemRarity.Config;
using ItemRarity.Items;
using ItemRarity.Logging;
using ItemRarity.Packets;
using ItemRarity.Recipes;
using ItemRarity.Server;
using ItemRarity.Server.Commands;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

// ReSharper disable MemberCanBePrivate.Global

namespace ItemRarity;

/// <summary>
/// Mod Entry point.
/// </summary>
[HarmonyPatch]
public sealed class ModCore : ModSystem
{
    public const string HarmonyId = "itemrarity.patches";
    public const string ConfigSyncNetChannel = "itemrarity.configsync";

    private static Harmony _harmonyInstance = null!;

    public static ModConfig Config { get; set; } = ModConfig.GetDefaultConfig();
    public static ICoreClientAPI? ClientApi { get; private set; }
    public static ICoreServerAPI? ServerApi { get; private set; }
    public static WeatherSystemServer? WeatherSystemServer { get; private set; }

    public override void Start(ICoreAPI api)
    {
        if (!Harmony.HasAnyPatches(HarmonyId))
        {
            Logger.Notification("Patching...");
            _harmonyInstance = new Harmony(HarmonyId);
            _harmonyInstance.PatchAll();
            Logger.Notification("Successfully patched. Starting mod...");
        }

        Config = ModConfig.Load(api);

        // Important for item comparison to ignore attributes e.g TreasureTrader for the story map.
        GlobalConstants.IgnoredStackAttributes = GlobalConstants.IgnoredStackAttributes.Append(AttributesManager.ModAttributeId);

        // Mods/Players may add custom attributes, if theses are attributes added outside the mod attribute tree we have to ignore them too.
        foreach (var rarity in Config.Rarity.Rarities.Values)
        {
            if (rarity.CustomAttributes is not { Count: > 0 } attributes)
                continue;
            var ignoreAttributes = attributes.Where(customAttribute => customAttribute.Key[0] != '$').Select(customAttribute => customAttribute.Key);
            GlobalConstants.IgnoredStackAttributes = GlobalConstants.IgnoredStackAttributes.Append(ignoreAttributes);
        }

        api.Network.RegisterChannel(ConfigSyncNetChannel).RegisterMessageType<ServerConfigMessage>();

        api.RegisterItemClass("ItemTier", typeof(ItemTier));
        api.RegisterItemClass("ItemTierOutput", typeof(ItemTierOutput));

        Logger.Notification("Mod Started.");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        ClientApi = api;

        api.Network.GetChannel(ConfigSyncNetChannel).SetMessageHandler<ServerConfigMessage>(packet =>
        {
            try
            {
                var config = JsonConvert.DeserializeObject<ModConfig>(packet.SerializedConfig);
                if (config != null)
                {
                    Config = config;
                    Logger.Notification("Received config from server.", EnumAppSide.Client);
                }
                else
                    Logger.Error("Received invalid config from server.", EnumAppSide.Client);
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

        api.Event.OnEntitySpawn += ServerEventsHandlers.OnEntitySpawn;
        api.Event.PlayerJoin += ServerEventsHandlers.OnPlayerJoin;

        api.RegisterCraftingRecipe(new TierUpgradeRecipe(api.World));

        WeatherSystemServer = api.ModLoader.GetModSystem<WeatherSystemServer>();

        var parsers = api.ChatCommands.Parsers;

        var mainCommand = api.ChatCommands.Create("rarity")
            .WithDescription("Commands related to ItemRarity mod")
            .RequiresPrivilege(Privilege.root)
            .RequiresPlayer();

        mainCommand.BeginSubCommand("set")
            .WithDescription("Change the currently held item rarity.")
            .WithArgs(parsers.Word("rarity", Config.Rarity.Rarities.Keys.ToArray()))
            .HandleWith(CommandsHandlers.HandleSetRarityCommand)
            .EndSubCommand();

        mainCommand.BeginSubCommand("reload")
            .WithDescription("Reload the configuration")
            .HandleWith(del => CommandsHandlers.HandleReloadConfigCommand(api, del))
            .EndSubCommand();

        mainCommand.BeginSubCommand("roll")
            .WithDescription("Run the random rarity generator.")
            .WithArgs(parsers.Int("times"), parsers.OptionalWord("tier"))
            .HandleWith(CommandsHandlers.HandleTestRarityCommandUnified)
            .EndSubCommand();

        mainCommand.BeginSubCommand("itemdebug")
            .WithDescription("Dev debug command")
            .HandleWith(CommandsHandlers.HandleDebugItemAttributesCommand)
            .EndSubCommand();
    }

    public override void Dispose()
    {
        _harmonyInstance.UnpatchAll(HarmonyId);
    }
}