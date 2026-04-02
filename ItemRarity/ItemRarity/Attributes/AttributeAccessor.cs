using ItemRarity.Logging;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ItemRarity.Attributes;

public sealed class AttributeAccessor(string attributeKey)
{
    private bool EnsureValid(ItemStack? itemStack, out ITreeAttribute modAttributes)
    {
        if (itemStack == null)
        {
            Logger.Warning($"Can't get attribute value '{Attribute.ModAttributeId}', ItemStack is null.'");
            modAttributes = null!;
            return false;
        }

        if (itemStack.Attributes == null)
        {
            Logger.Warning($"Can't get attribute value '{Attribute.ModAttributeId}' for ItemStack '{itemStack.Collectible.Code}' because the ItemStack's attributes are null.'");
            modAttributes = null!;
            return false;
        }

        var attributes = itemStack.Attributes.GetTreeAttribute(Attribute.ModAttributeId);
        if (attributes == null)
        {
            Logger.Warning(
                $"Can't get attribute value '{Attribute.ModAttributeId}' for ItemStack '{itemStack.Collectible.Code}' because the ItemStack's attributes are missing rarity.'");
            modAttributes = null!;
            return false;
        }

        modAttributes = attributes;
        return true;
    }

    public float GetFloat(ITreeAttribute attribute, float defaultValue = 0F)
    {
        return attribute.GetFloat(attributeKey, defaultValue);
    }

    public float GetFloat(ItemStack? itemStack, float defaultValue = 0F)
    {
        return !EnsureValid(itemStack, out var attributes) ? defaultValue : attributes.GetFloat(attributeKey, defaultValue);
    }

    public void SetFloat(ITreeAttribute attribute, float value)
    {
        attribute.SetFloat(attributeKey, value);
    }

    public void SetFloat(ItemStack? itemStack, float value)
    {
        if (!EnsureValid(itemStack, out var attributes))
            return;

        attributes.SetFloat(attributeKey, value);
    }
}