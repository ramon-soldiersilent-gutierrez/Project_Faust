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

}
