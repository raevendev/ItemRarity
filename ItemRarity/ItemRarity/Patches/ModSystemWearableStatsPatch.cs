using System;
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ModSystemWearableStats))]
public static class ModSystemWearableStatsPatch
{
    // I would like to avoid patching the whole method but i can't find any other way of doing that. 

    [HarmonyPrefix, HarmonyPatch("handleDamaged"), HarmonyPriority(Priority.Last)]
    public static bool HandleDamagedPatch(ModSystemWearableStats __instance, IPlayer player, float damage, DamageSource dmgSource, ref float __result,
        ref ICoreAPI ___api, ref Dictionary<int, EnumCharacterDressType[]> ___clothingDamageTargetsByAttackTacket, ref AssetLocation ___ripSound)
    {
        var type = dmgSource.Type;
        damage = ApplyShieldProtection(player, damage, dmgSource, ___api);
        if (damage <= 0.0)
        {
            __result = 0f;
            return false;
        }

        if (___api.Side == EnumAppSide.Client || type != EnumDamageType.BluntAttack && type != EnumDamageType.PiercingAttack && type != EnumDamageType.SlashingAttack ||
            dmgSource.Source == EnumDamageSource.Internal || dmgSource.Source == EnumDamageSource.Suicide)
        {
            __result = damage;
            return false;
        }

        var ownInventory = player.InventoryManager.GetOwnInventory("character");
        double num1;
        ItemSlot itemslot;
        int key;
        if ((num1 = ___api.World.Rand.NextDouble() - 0.2) < 0.0)
        {
            itemslot = ownInventory[12];
            key = 0;
        }
        else
        {
            if (num1 - 0.5 < 0.0)
            {
                itemslot = ownInventory[13];
                key = 1;
            }
            else
            {
                itemslot = ownInventory[14];
                key = 2;
            }
        }

        if (itemslot.Empty || !(itemslot.Itemstack.Item is ItemWearable) || itemslot.Itemstack.Collectible.GetRemainingDurability(itemslot.Itemstack) <= 0)
        {
            var characterDressTypeArray = ___clothingDamageTargetsByAttackTacket[key];
            var slotId = characterDressTypeArray[___api.World.Rand.Next(characterDressTypeArray.Length)];
            var slot = ownInventory[(int)slotId];
            if (!slot.Empty)
            {
                var num3 = 0.25f;
                if (type == EnumDamageType.SlashingAttack)
                    num3 = 1f;
                if (type == EnumDamageType.PiercingAttack)
                    num3 = 0.5f;
                var changeVal = (float)(-(double)damage / 100.0) * num3;
                if (Math.Abs(changeVal) > 0.05)
                    ___api.World.PlaySoundAt(___ripSound, player.Entity);
                if (slot.Itemstack.Collectible is ItemWearable collectible)
                    collectible.ChangeCondition(slot, changeVal);
            }

            __result = damage;
            return false;
        }

        var protectionModifiers = (itemslot.Itemstack.Item as ItemWearable)!.ProtectionModifiers;
        var damageTier = dmgSource.DamageTier;
        var flatDamageReduction = protectionModifiers.FlatDamageReduction *
                                  AttributesManager.GetStatsMultiplier(itemslot.Itemstack, AttributesManager.ArmorFlatDamageReductionMultiplier); // MODIFIED
        var relativeProtection = protectionModifiers.RelativeProtection;
        for (var index = 1; index <= damageTier; ++index)
        {
            var num4 = index > protectionModifiers.ProtectionTier ? 1 : 0;
            var num5 = num4 != 0 ? protectionModifiers.PerTierFlatDamageReductionLoss[1] : protectionModifiers.PerTierFlatDamageReductionLoss[0];
            num5 /= AttributesManager.GetStatsMultiplier(itemslot.Itemstack, AttributesManager.ArmorPerTierFlatDamageProtectionLossMultiplier); // MODIFIED
            var num6 = num4 != 0 ? protectionModifiers.PerTierRelativeProtectionLoss[1] : protectionModifiers.PerTierRelativeProtectionLoss[0];
            if (num4 != 0 && protectionModifiers.HighDamageTierResistant)
            {
                num5 /= 2f;
                num6 /= 2f;
            }

            flatDamageReduction -= num5;
            relativeProtection *= 1f - num6;
        }

        var amount = GameMath.RoundRandom(___api.World.Rand,
            (float)(0.5 + damage * (double)Math.Max(0.5f, (damageTier - protectionModifiers.ProtectionTier) * 3)));
        damage = Math.Max(0.0f, damage - flatDamageReduction);
        damage *= 1f - Math.Max(0.0f, relativeProtection);
        itemslot.Itemstack.Collectible.DamageItem(___api.World, player.Entity, itemslot, amount);
        if (itemslot.Empty)
            ___api.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), player);

        __result = damage;

        return false;
    }


    private static float ApplyShieldProtection(IPlayer player, float damage, DamageSource dmgSource, ICoreAPI api)
    {
        var num1 = 1.0471975803375244;
        var num2 = damage;
        var itemSlotArray = new[]
        {
            player.Entity.LeftHandItemSlot,
            player.Entity.RightHandItemSlot
        };
        for (var index = 0; index < itemSlotArray.Length; ++index)
        {
            var itemslot = itemSlotArray[index];
            var itemAttribute = itemslot.Itemstack?.ItemAttributes?["shield"];
            if (itemAttribute != null && itemAttribute.Exists)
            {
                var sourceEntity = dmgSource.SourceEntity;
                bool? nullable1;
                bool? nullable2;
                if (sourceEntity == null)
                {
                    nullable1 = new bool?();
                    nullable2 = nullable1;
                }
                else
                {
                    var attributes = sourceEntity.Properties.Attributes;
                    if (attributes == null)
                    {
                        nullable1 = new bool?();
                        nullable2 = nullable1;
                    }
                    else
                        nullable2 = attributes["isProjectile"].AsBool();
                }

                nullable1 = nullable2;
                var valueOrDefault = nullable1.GetValueOrDefault();
                var key1 = !player.Entity.Controls.Sneak || player.Entity.Attributes.GetInt("aiming", 0) == 1 ? "passive" : "active";
                float num3;
                float num4;
                if (valueOrDefault && itemAttribute["protectionChance"][key1 + "-projectile"].Exists)
                {
                    num3 = itemAttribute["protectionChance"][key1 + "-projectile"].AsFloat();
                    num4 = itemAttribute["projectileDamageAbsorption"].AsFloat(2f) *
                           AttributesManager.GetStatsMultiplier(itemslot.Itemstack, AttributesManager.ShieldProjectileDamageAbsorptionMultiplier); // MODIFIED
                }
                else
                {
                    num3 = itemAttribute["protectionChance"][key1].AsFloat();
                    num4 = itemAttribute["damageAbsorption"].AsFloat(2f) *
                           AttributesManager.GetStatsMultiplier(itemslot.Itemstack, AttributesManager.ShieldDamageAbsorptionMultiplier); // MODIFIED
                }

                double attackYaw;
                double attackPitch;
                if (dmgSource.GetAttackAngle(player.Entity.Pos.XYZ, out attackYaw, out attackPitch))
                {
                    var flag = Math.Abs(attackPitch) > 1.1344640254974365;
                    var yaw = (double)player.Entity.Pos.Yaw;
                    var pitch = (double)player.Entity.Pos.Pitch;
                    if (valueOrDefault)
                    {
                        var x = dmgSource.SourceEntity.SidedPos.Motion.X;
                        var y = dmgSource.SourceEntity.SidedPos.Motion.Y;
                        var z = dmgSource.SourceEntity.SidedPos.Motion.Z;
                        flag = Math.Sqrt(x * x + z * z) < Math.Abs(y);
                    }

                    if (!flag
                            ? Math.Abs(GameMath.AngleRadDistance((float)yaw, (float)attackYaw)) < num1
                            : Math.Abs(GameMath.AngleRadDistance((float)pitch, (float)attackPitch)) < 0.5235987901687622)
                    {
                        var val1 = 0.0f;
                        var num5 = api.World.Rand.NextDouble();
                        if (num5 < num3)
                            val1 += num4;
                        if (player is IServerPlayer serverPlayer)
                        {
                            var damageLogChatGroup = GlobalConstants.DamageLogChatGroup;
                            var message = Lang.Get("{0:0.#} of {1:0.#} damage blocked by shield ({2} use)", Math.Min(val1, damage), damage,
                                key1);
                            serverPlayer.SendMessage(damageLogChatGroup, message, EnumChatType.Notification);
                        }

                        damage = Math.Max(0.0f, damage - val1);
                        var key2 = "blockSound" + (num2 > 6.0 ? "Heavy" : "Light");
                        api.World.PlaySoundAt(
                            AssetLocation.Create(itemslot.Itemstack!.ItemAttributes!["shield"][key2].AsString("held/shieldblock-wood-light"),
                                itemslot.Itemstack.Collectible.Code.Domain).WithPathPrefixOnce("sounds/").WithPathAppendixOnce(".ogg"), player);
                        if (num5 < num3)
                            (api as ICoreServerAPI)!.Network.BroadcastEntityPacket(player.Entity.EntityId, 200,
                                SerializerUtil.Serialize("shieldBlock" + (index == 0 ? "L" : "R")));
                        if (api.Side == EnumAppSide.Server)
                        {
                            itemslot.Itemstack.Collectible.DamageItem(api.World, dmgSource.SourceEntity, itemslot);
                            itemslot.MarkDirty();
                        }
                    }
                }
                else
                    break;
            }
        }

        return damage;
    }
}