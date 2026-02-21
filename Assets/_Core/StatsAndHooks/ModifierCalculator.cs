using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.StatsAndHooks
{
    // Owns the PoB Math resolution logic and AbilityContext assembly.
    public class ModifierCalculator : MonoBehaviour
    {
        public static ModifierCalculator Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public AbilityContext BuildContext(SkillDefinitionSO skillDef, List<SpecializationNodeSO> activeNodes)
        {
            AbilityContext ctx = new AbilityContext
            {
                ExecutionShape = skillDef.ExecutionShape
            };

            // Initialize buckets with base values
            StatPackage damagePack = new StatPackage(skillDef.BaseDamage);
            StatPackage castTimePack = new StatPackage(skillDef.BaseCastTime);
            StatPackage projSpeedPack = new StatPackage(skillDef.BaseProjectileSpeed);
            StatPackage projCountPack = new StatPackage(skillDef.BaseProjectileCount);
            
            // Loop through all data nodes and stack the modifiers
            foreach (var node in activeNodes)
            {
                // Validate tags before applying (Agent D safety net)
                if (node.RequiredTags != GemTag.None && !skillDef.HasTag(node.RequiredTags))
                {
                    Debug.LogWarning($"Skipped Node {node.NodeID} - Skill {skillDef.SkillID} lacks required tag {node.RequiredTags}");
                    continue; // Skip invalid application
                }

                damagePack.AddFlat(node.AddDamageFlat);
                damagePack.AddIncreased(node.AddDamageIncreased);
                damagePack.AddMore(node.AddDamageMore);

                castTimePack.AddIncreased(node.AddCastSpeedIncreased);
                projCountPack.AddFlat(node.AddProjectileCount);
                projSpeedPack.AddIncreased(node.AddProjectileSpeedIncreased);
            }

            // Bake final clamped numbers
            ctx.FinalDamage = Mathf.Clamp(damagePack.Resolve(), 1f, 99999f);
            ctx.FinalCastTime = Mathf.Clamp(castTimePack.Resolve(), 0.05f, 3f);
            ctx.FinalProjectileSpeed = projSpeedPack.Resolve();
            ctx.FinalProjectileCount = (int)Mathf.Clamp(projCountPack.Resolve(), 1f, 15f);

            return ctx;
        }
    }
}
