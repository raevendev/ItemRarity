using ItemRarity.Items;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace ItemRarity.Behaviors;

public sealed class CollectibleBehaviorTier(CollectibleObject collObj) : CollectibleBehavior(collObj)
{
    private static float _tierApplicationDuration = 3f;

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent,
        ref EnumHandHandling handHandling,
        ref EnumHandling handling)
    {
        if (!firstEvent || handHandling == EnumHandHandling.PreventDefault)
            return;

        if (byEntity.LeftHandItemSlot?.Itemstack?.Collectible is not ItemTier || !Rarity.IsSuitableFor(byEntity.ActiveHandItemSlot.Itemstack))
            return;
        
        byEntity.StartAnimation("insertgear");

        handHandling = EnumHandHandling.PreventDefault;
    }

    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel,
        ref EnumHandling handling)
    {
        handling = EnumHandling.PreventDefault;
        return secondsUsed < _tierApplicationDuration;
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel,
        ref EnumHandling handling)
    {
        if (!(secondsUsed >= 3))
            return;
        if (byEntity.LeftHandItemSlot?.Itemstack?.Collectible is not ItemTier tier || !Rarity.IsSuitableFor(byEntity.ActiveHandItemSlot.Itemstack))
            return;

        ModLogger.Notification("USED");

        if (byEntity.World.Side == EnumAppSide.Client)
            return;

        Rarity.SetRarityByTier(byEntity.ActiveHandItemSlot.Itemstack, tier.Code.EndVariant());
        byEntity.LeftHandItemSlot?.TakeOut(1);
        byEntity.ActiveHandItemSlot.MarkDirty();
    }
}