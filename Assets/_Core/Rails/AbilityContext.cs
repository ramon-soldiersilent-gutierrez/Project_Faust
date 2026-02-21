using System;
using UnityEngine;

namespace Faust.Rails
{
    [Flags]
    public enum GemTag 
    {
        None = 0,
        Projectile = 1 << 0,
        Melee = 1 << 1,
        Spell = 1 << 2,
        Attack = 1 << 3,
        AoE = 1 << 4,
        Movement = 1 << 5
    }

    public enum SkillShape 
    {
        Projectile, 
        ForwardSweep,
        TargetedAoE
    }

    // Agent C & B: This context is built from the SkillDefinitionSO + SpecializationNodeSOs + Player Stats
    public struct AbilityContext
    {
        public SkillShape ExecutionShape;
        
        public float FinalDamage;
        public float FinalCastTime;
        public float FinalProjectileSpeed;
        public float FinalAreaRadius;
        public int FinalProjectileCount;
    }
}
