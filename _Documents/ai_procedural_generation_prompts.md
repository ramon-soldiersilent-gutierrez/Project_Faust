# Phase 7: AI Procedural Generation Prompts (Agent D Handoff)

With the expansion into a full ARPG, Agent D needs to update the `ContractModel` to handle **Inventory Items** and build a brand new schema for the **Procedural Skill Tree**.

Here are the revised schemas and prompts to hand to Agent D.

---

## 1. The Inventory Item Prompt Update

**The Goal:** We need the generated items to include a generic keyword so Agent C's UI can map it to a sprite from a free asset pack (e.g., "Sword", "Amulet", "Armor", "Ring").

### Updated JSON Schema (C#)
```csharp
[System.Serializable]
public class ContractModel
{
    public string ItemName;
    public string FlavorText;
    public string EquipSlot; // e.g. "Weapon", "Armor", "Accessory"
    public string SpriteKeyword; // e.g. "Sword_Dark", "Ring_Blood", "Chest_Iron"
    
    public SkillType GrantedSkill; 
    public BoonType[] Boons;
    public CurseType[] Curses;
    
    public float DamageModifier;
    public float SpeedModifier;
    public float SizeModifier;
}
```

### Updated Gemini Prompt Fragment
```text
...
Rules:
1. You MUST respond ONLY in valid JSON matching the schema below.
2. Assign an EquipSlot from: "Weapon", "Armor", "Accessory".
3. Provide a SpriteKeyword that conceptually matches the item. Use the format "[BaseType]_[Theme]". Example: "Sword_Demonic", "Amulet_Bone", "Armor_Spiked". We will use this to dynamically load 2D UI sprites.
...
```

---

## 2. The Procedural Skill Tree Prompt (The "Faustian Web")

**The Concept:** Every 5 levels, the AI generates a new "chunk" of the skill tree containing 15-30 interconnected nodes. The player earns 2 skill points per level (10 points total per chunk), forcing them to pick a path through the generated web without locking them out if they want to backtrack later. 

Nodes can be minor stat bumps or major Keystones (Boons/Curses).

### Skill Tree JSON Schema (C#)
```csharp
[System.Serializable]
public class SkillTreeChunk
{
    public string ChunkName; // e.g., "The Bloodstained Path"
    public string Theme;
    public SkillTreeNode[] Nodes;
}

[System.Serializable]
public class SkillTreeNode
{
    public string NodeID; // Unique ID, e.g., "node_blood_1"
    public string DisplayName;
    public string FlavorText;
    
    // Graph Connections
    public string[] ConnectedNodeIDs; 
    
    // Visual Plotting (Relative Grid Coordinates)
    public int GridX;
    public int GridY;

    // Node Type Flag
    public bool IsKeystone; // True if it contains Boons/Curses
    
    // Payloads
    public BoonType[] GrantedBoons;
    public CurseType[] GrantedCurses;
    public float DamageDelta; // e.g., +0.05 for 5% increased damage
    public float SpeedDelta;
    public float SizeDelta;
}
```

### Gemini Prompt for Skill Tree Generation
```text
You are the Faustian Forge. The mortal player has reached a milestone and demands a new path of power. You must generate a new 2D web of Skill Tree Nodes.

Mortal's Current Level: {player_level}
Mortal's Equipped Skills: {current_skills}
Mortal's Theme Preference: "{user_prompt_theme}"

Rules:
1. Generate exactly 15 to 30 interconnected nodes. 
2. Arrange them logically using GridX and GridY coordinates. Node 0 should be at (0,0) and connect to the existing tree.
3. Every node MUST have at least 1 connection in `ConnectedNodeIDs` linking it to another node in this chunk.
4. 80% of nodes should be minor stat bumps (DamageDelta, SpeedDelta). 
5. 20% of nodes MUST be major "Keystones" (`IsKeystone`: true). Keystones MUST grant 1-2 Boons and 1-2 crippling Curses to maintain the Monkey's Paw pact.
6. Thematic consistency: The `FlavorText` and `DisplayName` of the nodes should reflect a descent into dark, forbidden magic.

Respond ONLY with valid JSON matching the SkillTreeChunk schema.
```
