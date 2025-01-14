**Item Rarity Mod**

This mod introduces an exciting **rarity mechanic** to items, adding a dynamic twist to the crafting and looting experience. Rarity levels influences key item stats, such as durability, damage, and more. Currently, rarities are applied exclusively to tools when they are crafted or dropped.

### **Features**

#### **Stat Modifications**

Rarities affect the default stats of tools, enhancing or altering their performance.

**Supported Stats:**

*   **Durability**: All tools benefit from durability adjustments.
*   **Mining Speed**: Applies to tools capable of mining.
*   **Attack Power**: Influences the strength of all tools in combat.
*   **Piercing Power**: Impacts tools that deal piercing damage.

#### **Magical Effects**

Bring a touch of magic to your world! Rarity levels can grant special effects to tools, with the first effect now available:

*   **Thor**: A powerful effect for tools that deal piercing damage. Strikes targets with lightning and sets them ablaze on impact.

### **Commandsa**

There a several commands available. (For now you have to be an operator to be able to use thoses commands)

*   _/rarity set <rarity>_ : Applies the specified rarity to the item currently held.
    
*   _/rarity reload_ : Reloads the configuration without needing to restart the game or server, ideal for testing new settings.
*   _/rarity test <times>_ : Perform a test that will generate X rarities based on the times provided. Ideal to test configured rarities. It will also display their relative chance.

### **Planned Features**

*   Introduction of additional effects.
    
*   Enhanced configuration options to limit rarity application to specific items.
    

### **Configuration**

Rarity levels can be customized to suit your preferences. Each rarity level has several configurable properties, allowing you to fine-tune its effects and influence on the game. Below is an example of how to configure a rarity:

#### **Example Configuration**

    "unique": {
      "Name": "Unique",
      "Color": "#EC290E",
      "Rarity": 2.0,
      "DurabilityMultiplier": 2.0,
      "MiningSpeedMultiplier": 1.9,
      "AttackPowerMultiplier": 1.9,
      "PiercingPowerMultiplier": 1.9,
      "Effects": ["Thor"],
      "SupportedItems": ["*"]
    }
    

#### **Explanation of Fields**

*   **`Name`**: The name of the rarity (e.g., "Unique"). This will be displayed in-game (might be overrided by land files).
*   **`Color`**: Hexadecimal color code used to represent the rarity.
*   **`Rarity`**: The weight or chance of this rarity being applied. Lower values make the rarity rarer.
*   **`DurabilityMultiplier`**: Multiplier for the tool's durability.
*   **`MiningSpeedMultiplier`**: Multiplier for the tool's mining speed.
*   **`AttackPowerMultiplier`**: Multiplier for the tool's attack power.
*   **`PiercingPowerMultiplier`**: Multiplier for the tool's piercing power.
*   **`Effects`**: A list of special effects applied to tools of this rarity. Example: `"Thor"`.
*   **`SupportedItems`**: **NOT YET SUPPORTED** Specifies which items this rarity can be applied to. Use `"*"` to apply to all tools or list specific tools (e.g., `["pickaxe", "axe"]`).

#### **Adding New Rarities**

To add a new rarity, simply follow the same format.

    "mynewrarity": {
      "Name": "My Awesome Rarity",
      "Color": "#4A90E2",
      "Rarity": 5.0,
      "DurabilityMultiplier": 1.5,
      "MiningSpeedMultiplier": 1.4,
      "AttackPowerMultiplier": 1.4,
      "PiercingPowerMultiplier": 1.4,
      "Effects": [],
      "SupportedItems": ["*"]
    }

This allows full control over how each rarity impacts gameplay, ensuring a tailored experience for your world.

* * *

### **Inspiration**

This mod is inspired by the **[RPG Item Rarity](https://mods.vintagestory.at/rpgitemrarity)** mod.
