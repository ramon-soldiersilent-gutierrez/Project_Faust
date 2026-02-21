using System;
using UnityEngine;

namespace Faust.Rails
{
    // The data representation of the LLM's chosen contract. Agent D creates this, Agent C consumes it.
    [Serializable]
    public class ContractModel
    {
        public string ItemName;
        public string FlavorText;
        public string EquipSlot; // e.g. "Weapon", "Armor", "Accessory"
        public string SpriteKeyword; // e.g. "Sword_Dark", "Ring_Blood", "Chest_Iron"
        
        public string GrantedSkillID; // e.g., "Kinetic_Projectile"
        
        public string[] BoonNodeIDs; // e.g., ["Boon_DamageSpike", "Boon_Multicast"]
        public string[] CurseNodeIDs; // e.g., ["Curse_GlassCannon"]
        
        // Raw floats from the AI, which MUST be clamped by Agent D before handing to Agent C.
        public float DamageModifier = 1.0f;
        public float SpeedModifier = 1.0f;
        public float SizeModifier = 1.0f;
    }

    [Serializable]
    public class SkillTreeChunk
    {
        public string ChunkName; // e.g., "The Bloodstained Path"
        public string Theme;
        public SkillTreeNode[] Nodes;
    }

    [Serializable]
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
        public string[] GrantedBoons;
        public string[] GrantedCurses;
        public float DamageDelta; // e.g., +0.05 for 5% increased damage
        public float SpeedDelta;
        public float SizeDelta;
    }
}
