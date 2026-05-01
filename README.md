# Item Rarity Mod

Add excitement and variety to your **Vintage Story** experience with the **Item Rarity Mod**! This mod introduces a \*
\*rarity system\*\* for tools, affecting their stats and adding special effects. Every time you craft or find a tool, it
has a chance to receive a rarity making each item feel unique and powerful!

---

## Features

### Stat Modifiers

Each rarity level modifies core tool stats, making tools stronger or more efficient based on their rarity.

**Affected stats:**

- **Durability** - Increase or decrease items durability.
- **Mining Speed** - Increase or decrease mining speed.
- **Attack Power** - Increase or decrease damage dealt.
- **Attack Range** - Increase or decrease the reach of melee weapons.
- **Piercing Power** - Increase or decrease damage dealt by piercing attacks (thrown spears, arrows, etc.).
- **Armor Flat Damage Reduction** - Increase or decrease flat damage reduction on armor.
- **Shield Protection** - Increase or decrease the effectiveness of shields.

---

### Magical Effects

Rarities can also grant **magical effects**. These effects can add dramatic new abilities to your tools.

**Available effects:**

- **Thor** – When hitting a target with a piercing weapon, strikes lightning and sets the target on fire.

---

## Commands

> ⚠️ All commands require **operator** (admin) permissions.

| Command                 | Description                                                              |
| ----------------------- | ------------------------------------------------------------------------ |
| `/rarity set <rarity>`  | Assigns the specified rarity to the item currently held.                 |
| `/rarity reload`        | Reloads the configuration without restarting the game/server.            |
| `/rarity roll <times> ` | Simulates multiple rarity rolls and displays results with probabilities. |

---

## Configuration

Customize rarities to match your server's balance or your personal gameplay style. Each rarity level supports detailed
customization. You can create as many rarities as you want and adjust their stats to balance gameplay your way.

> Multipliers supports fixed number e.g `1.5` or range e.g `[0.2, 0.9]`. When using a range, the multiplier is chosen randomly within the range.

### Example Configuration

```json
"my-super-rarity": {
  "Key": "my-super-rarity",
  "Name": "Super Rarity",
  "Level": 4,
  "Color": "#606060",
  "Weight": 8.0,
  "DurabilityMultiplier": [0.2, 0.9],
  "MiningSpeedMultiplier": 1.5,
  "AttackPowerMultiplier": 1.5,
  "AttackRangeMultiplier": 1.5,
  "PiercingPowerMultiplier": 1.5,
  "ArmorFlatDamageReductionMultiplier": 1.5,
  "ArmorPerTierFlatDamageProtectionLossMultiplier": 1.5,
  "ShieldProtectionMultiplier": 2,
  "CustomAttributes": {},
  "Effects": ["Thor"],
  "IgnoreTranslation": false
}
```

| Field                                            | Description                                                                                                                                          |
| ------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Key`                                            | Key used by the mod to identify rarities. **_BE CAREFUL CHANGING THIS CAN CAUSE PROBLEMS_**                                                          |
| `Name`                                           | Display name of the rarity (can be localized).                                                                                                       |
| `Level`                                          | Numerical rarity level (It is a numerical representation of the rarity).                                                                             |
| `Color`                                          | Hex color code shown in item display.                                                                                                                |
| `Weight`                                         | Used for selection. Higher = more common. **_(See "How Rarity Works" below)_**                                                                       |
| `DurabilityMultiplier`                           | Affects durability.                                                                                                                                  |
| `MiningSpeedMultiplier`                          | Affects mining speed.                                                                                                                                |
| `AttackPowerMultiplier`                          | Affects melee damage.                                                                                                                                |
| `AttackRangeMultiplier`                          | Affects weapon reach.                                                                                                                                |
| `PiercingPowerMultiplier`                        | Affects piercing damage.                                                                                                                             |
| `ArmorFlatDamageReductionMultiplier`             | Affects armor flat damage reduction.                                                                                                                 |
| `ArmorPerTierFlatDamageProtectionLossMultiplier` | Affects armor flat damage reduction per tier.                                                                                                        |
| `ShieldProtectionMultiplier`                     | Affects shield damage reduction per tier.                                                                                                            |
| `CustomAttributes`                               | Useful for mods, thoses will be stored in the item metadata. Starting the attribute with a **$** will store it within the the rarity tree attribute. |
| `Effects`                                        | List of special abilities. Example: `["Thor"]`.                                                                                                      |
| `IgnoreTranslation`                              | Name field will not be localized.                                                                                                                    |

---

## How Rarity Works

The `Rarity` value in the config is used as a **weight**, not a percentage. You **do not** need to make them add up to 100.

### Key Points:

- Higher values = more likely to be selected.
- The actual chance is based on the **relative weight** compared to others.
- Randomized Stats: If a multiplier is defined as a range (e.g., [0.5, 1.5]), the mod will roll a random value within that range every time an item of that rarity is generated.

Example:

```json
"Common":   { "Weight": 80.0 },
"Uncommon": { "Weight": 15.0 },
"Rare":     { "Weight": 4.0 },
"Legendary":{ "Weight": 1.0 }
```

Here, **Common** has an 80% chance because 80 / (80 + 15 + 4 + 1) = 0.80

---

## Planned Features

- More **special effects** to diversify tool behavior.
- Advanced item filtering via.
- A more linear way of getting rarities, more tied to the player progression than total randomness.

---

## Inspiration

Inspired by the [RPG Item Rarity Mod](https://mods.vintagestory.at/rpgitemrarity).

---
