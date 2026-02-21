using System;
using UnityEngine;

namespace Faust.Rails
{
    [CreateAssetMenu(menuName = "Faust/Skill Definition")]
    public class SkillDefinitionSO : ScriptableObject 
    {
        public string SkillID; // e.g., "Kinetic_Projectile"
        public GemTag Tags;    // e.g., GemTag.Projectile | GemTag.Spell
        public SkillShape ExecutionShape;

        [Header("Base Stats")]
        public float BaseDamage = 10f;
        public float BaseCastTime = 0.5f;
        public float BaseProjectileSpeed = 20f; 
        public float BaseAreaRadius = 1f;       
        public int BaseProjectileCount = 1;

        // Validation helper
        public bool HasTag(GemTag tag) => (Tags & tag) != 0;
    }

    [CreateAssetMenu(menuName = "Faust/Specialization Node")]
    public class SpecializationNodeSO : ScriptableObject 
    {
        public string NodeID; // Matches LLM Output (e.g., "Boon_Multicast")
        public GemTag RequiredTags; // Fails to apply if skill lacks these
        public GemTag ExcludedTags; // Fails to apply if skill has these

        [Header("Stat Deltas")]
        public float AddDamageFlat = 0f;
        public float AddDamageIncreased = 0f; 
        public float AddDamageMore = 1f;      // Remember: 1f means 0% More. 
        
        public float AddCastSpeedIncreased = 0f;
        public int AddProjectileCount = 0;
        public float AddProjectileSpeedIncreased = 0f;

        [Header("Behavior Hooks")]
        [Tooltip("The ID of the IHookInstance Agent B should inject when this node is applied.")]
        public string InjectHookID = ""; 
    }
}
