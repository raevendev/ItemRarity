# 🪄 Item Rarity Mod

Add excitement and variety to your **Vintage Story** experience with the **Item Rarity Mod**! This mod introduces a *
*rarity system** for tools, affecting their stats and adding special effects. Every time you craft or find a tool, it
has a chance to receive a rarity making each item feel unique and powerful!

---

## 🌟 Features

### 🔧 Stat Modifiers

Each rarity level modifies core tool stats, making tools stronger or more efficient based on their rarity.

**Affected stats:**

* **Durability** - Modifies the lifespan of tools.
* **Mining Speed** - Affects how fast you mine with the tool.
* **Attack Power** - Modifies damage dealt in combat.
* **Piercing Power** - Modifies damage dealt by piercing attacks, such as thrown spears, arrows, and other.

---

### ⚡ Magical Effects

Rarities can also grant **magical effects**. These effects can add dramatic new abilities to your tools.

**Available effects:**

* **Thor** – When hitting a target with a piercing weapon, strikes lightning and sets the target on fire.

---

## 💬 Commands

> ⚠️ All commands require **operator** (admin) permissions.

| Command                | Description                                                              |
|------------------------|--------------------------------------------------------------------------|
| `/rarity set <rarity>` | Assigns the specified rarity to the item currently held.                 |
| `/rarity reload`       | Reloads the configuration without restarting the game/server.            |
| `/rarity test <times>` | Simulates multiple rarity rolls and displays results with probabilities. |

---

## 🛠️ Configuration

Customize rarities to match your server's balance or your personal gameplay style. Each rarity level supports detailed
customization.

### 📄 Example Configuration

```json
"unique": {
"Name": "Unique",
"Color": "#EC290E",
"Weight": 2.0,
"DurabilityMultiplier": 2.0,
"MiningSpeedMultiplier": 1.9,
"AttackPowerMultiplier": 1.9,
"PiercingPowerMultiplier": 1.9,
"Effects": ["Thor"],
"SupportedItems": ["*"]
}
```

### 🧾 Explanation of Fields

| Field                     | Description                                                                             |
|---------------------------|-----------------------------------------------------------------------------------------|
| `Key`                     | Key used by the mod to identify rarities. *BE CAREFUL CHANGING THIS CAN CAUSE PROBLEMS* |
| `Name`                    | Display name of the rarity (can be localized).                                          |
| `Color`                   | Hex color code shown in item display.                                                   |
| `Weight`                  | **Weight** used for selection. Higher = more common. *(See "How Rarity Works" below)*   |
| `DurabilityMultiplier`    | Affects how long tools last.                                                            |
| `MiningSpeedMultiplier`   | Affects block-breaking speed.                                                           |
| `AttackPowerMultiplier`   | Affects melee damage.                                                                   |
| `PiercingPowerMultiplier` | Affects piercing (armor-bypassing) damage.                                              |
| `Effects`                 | List of special abilities. Example: `["Thor"]`.                                         |

---

## 🎯 How Rarity Works

The `Rarity` value in the config is used as a **weight**, not a percentage. You **do not** need to make them add up to

100.

### ✅ Key Points:

* Higher values = more likely to be selected.
* The actual chance is based on the **relative weight** compared to others.
* Example:

  ```json
  "Common":   { "Weight": 80.0 },
  "Uncommon": { "Weight": 15.0 },
  "Rare":     { "Weight": 4.0 },
  "Legendary":{ "Weight": 1.0 }
  ```

  Here, **Common** has an 80% chance because 80 / (80 + 15 + 4 + 1) = 0.80

---

## ➕ Adding New Rarities

Just add a new entry in the config following the same format:

```json
{
  "Key": "epic",
  "Name": "Epic",
  "Color": "#A020F0",
  "Weight": 0.5,
  "DurabilityMultiplier": 2.5,
  "MiningSpeedMultiplier": 2.0,
  "AttackPowerMultiplier": 2.2,
  "PiercingPowerMultiplier": 2.0,
  "Effects": ["Thor"],
  "SupportedItems": ["*"]
}
```

Create as many rarities as you want and adjust their stats to balance gameplay your way.

---

## 🚧 Planned Features

* More **special effects** to diversify tool behavior.
* Advanced item filtering via `SupportedItems`.
* Rarity support for **armor, weapons, and non-tool items**.

---

## 🎨 Inspiration

Inspired by the [RPG Item Rarity Mod](https://mods.vintagestory.at/rpgitemrarity).

---
