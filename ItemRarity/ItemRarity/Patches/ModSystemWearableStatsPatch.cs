using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Attribute = ItemRarity.Attributes.Attribute;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable NotAccessedVariable
// ReSharper disable SuggestVarOrType_BuiltInTypes

// ReSharper disable InconsistentNaming

namespace ItemRarity.Patches;

[HarmonyPatch(typeof(ModSystemWearableStats))]
public static class ModSystemWearableStatsPatch
{
    private static float ApplyShieldProtection(IPlayer player, float damage, DamageSource dmgSource, ICoreAPI api)
    {
        double num1 = 1.0471975803375244;
        float num2 = damage;
        ItemSlot[] itemSlotArray =
        [
            player.Entity.LeftHandItemSlot,
            player.Entity.RightHandItemSlot
        ];
        for (int index = 0; index < itemSlotArray.Length; ++index)
        {
            ItemSlot itemSlot = itemSlotArray[index];
            JsonObject itemAttribute = itemSlot.Itemstack?.ItemAttributes?["shield"];
            if (itemAttribute != null && itemAttribute.Exists)
            {
                Entity sourceEntity = dmgSource.SourceEntity;
                bool? nullable1;
                bool? nullable2;
                if (sourceEntity == null)
                {
                    nullable1 = new bool?();
                    nullable2 = nullable1;
                }
                else
                {
                    JsonObject attributes = sourceEntity.Properties.Attributes;
                    if (attributes == null)
                    {
                        nullable1 = new bool?();
                        nullable2 = nullable1;
                    }
                    else
                        nullable2 = new bool?(attributes["isProjectile"].AsBool());
                }

                nullable1 = nullable2;
                bool valueOrDefault = nullable1.GetValueOrDefault();
                string key1 = !player.Entity.Controls.Sneak || player.Entity.Attributes.GetInt("aiming", 0) == 1 ? "passive" : "active";
                float num3;
                float num4;
                if (valueOrDefault && itemAttribute["protectionChance"][key1 + "-projectile"].Exists)
                {
                    num3 = itemAttribute["protectionChance"][key1 + "-projectile"].AsFloat();
                    num4 = itemAttribute["projectileDamageAbsorption"].AsFloat(2f) *
                           Attribute.ShieldProjectileDamageAbsorptionMultiplier.GetFloat(itemSlot.Itemstack, 1f); // MODIFIED
                }
                else
                {
                    num3 = itemAttribute["protectionChance"][key1].AsFloat();
                    num4 = itemAttribute["damageAbsorption"].AsFloat(2f) *
                           Attribute.ShieldDamageAbsorptionMultiplier.GetFloat(itemSlot.Itemstack, 1f); // MODIFIED
                }

                double attackYaw;
                double attackPitch;
                if (dmgSource.GetAttackAngle(player.Entity.Pos.XYZ, out attackYaw, out attackPitch))
                {
                    bool flag = Math.Abs(attackPitch) > 1.1344640254974365;
                    double yaw = player.Entity.Pos.Yaw;
                    double start = 3.1415927410125732 - player.Entity.Pos.Pitch;
                    if (valueOrDefault)
                    {
                        double x = dmgSource.SourceEntity.Pos.Motion.X;
                        double y = dmgSource.SourceEntity.Pos.Motion.Y;
                        double z = dmgSource.SourceEntity.Pos.Motion.Z;
                        flag = Math.Sqrt(x * x + z * z) * 1.2000000476837158 < Math.Abs(y);
                    }

                    if (!flag
                            ? Math.Abs(GameMath.AngleRadDistance((float)yaw, (float)attackYaw)) < num1
                            : Math.Abs(GameMath.AngleRadDistance((float)start, (float)attackPitch)) < 0.5235987901687622)
                    {
                        float val1 = 0.0f;
                        double num5 = api.World.Rand.NextDouble();
                        if (num5 < num3)
                            val1 += num4;
                        if (player is IServerPlayer serverPlayer)
                        {
                            int damageLogChatGroup = GlobalConstants.DamageLogChatGroup;
                            string message = Lang.Get("absorbed-damageamount-of-total-damageamount-blocked-by-shield-in-mode", Math.Min(val1, damage),
                                damage, key1);
                            serverPlayer.SendMessage(damageLogChatGroup, message, EnumChatType.Notification);
                        }

                        damage = Math.Max(0.0f, damage - val1);
                        string key2 = "blockSound" + (num2 > 6.0 ? "Heavy" : "Light");
                        api.World.PlaySoundAt(
                            AssetLocation.Create(itemSlot.Itemstack.ItemAttributes["shield"][key2].AsString("held/shieldblock-wood-light"),
                                itemSlot.Itemstack.Collectible.Code.Domain).WithPathPrefixOnce("sounds/").WithPathAppendixOnce(".ogg"), player);
                        if (num5 < num3)
                            (api as ICoreServerAPI).Network.BroadcastEntityPacket(player.Entity.EntityId, 200,
                                SerializerUtil.Serialize<string>("shieldBlock" + (index == 0 ? "L" : "R")));
                        if (api.Side == EnumAppSide.Server)
                        {
                            itemSlot.Itemstack.Collectible.DamageItem(api.World, dmgSource.SourceEntity, itemSlot);
                            itemSlot.MarkDirty();
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