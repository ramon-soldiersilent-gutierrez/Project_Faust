using System;
using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.AI
{
    [Serializable]
    public class RawContractNode
    {
        public string id;
        public float magnitude;
    }

    [Serializable]
    public class RawContractJson
    {
        public string itemName;
        public string flavorText;
        public string skillPref;
        public string[] tags;
        public RawContractNode[] boons;
        public RawContractNode[] curses;
        
        // Include modifiers since they exist in ContractModel. 
        // The LLM will generate them.
        public float damageModifier = 1.0f;
        public float speedModifier = 1.0f;
        public float sizeModifier = 1.0f;
    }

    public static class ContractParser
    {
        private static readonly HashSet<string> ValidSkills = new HashSet<string>
        {
            "Kinetic_Projectile", "Kinetic_Sweep"
        };

        private static readonly HashSet<string> ValidBoons = new HashSet<string>
        {
            "Boon_DamageSpike", "Boon_MachineGun", "Boon_Multicast", "Boon_Vampiric"
        };

        private static readonly HashSet<string> ValidCurses = new HashSet<string>
        {
            "Curse_TeleportOnHit", "Curse_GlassCannon", "Curse_SelfDamage", "Curse_Rooted"
        };

        public static ContractModel ParseAndValidate(string json, ILogSink logger = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON is null or empty.");
            }

            // Strip markdown formatting if the LLM wrapped it in ```json ... ```
            string cleanJson = CleanJsonString(json);
            
            RawContractJson raw = JsonUtility.FromJson<RawContractJson>(cleanJson);
            if (raw == null)
            {
                throw new Exception("Failed to parse JSON into expected shape.");
            }

            ContractModel model = new ContractModel
            {
                ItemName = string.IsNullOrEmpty(raw.itemName) ? "Unknown Item" : raw.itemName,
                FlavorText = string.IsNullOrEmpty(raw.flavorText) ? "..." : raw.flavorText,
            };

            // Enforce Skill Whitelist
            if (ValidSkills.Contains(raw.skillPref))
            {
                model.GrantedSkillID = raw.skillPref;
            }
            else
            {
                logger?.LogWarning($"Unknown or missing Skill ID dropped: '{raw.skillPref}'");
                model.GrantedSkillID = "Kinetic_Projectile"; // Fallback to default
            }

            // Enforce Boons Whitelist
            List<string> validBoonIds = new List<string>();
            if (raw.boons != null)
            {
                foreach (var boon in raw.boons)
                {
                    if (ValidBoons.Contains(boon.id))
                        validBoonIds.Add(boon.id);
                    else
                        logger?.LogWarning($"Unknown Boon ID dropped: '{boon.id}'");
                }
            }
            model.BoonNodeIDs = validBoonIds.ToArray();

            // Enforce Curses Whitelist
            List<string> validCurseIds = new List<string>();
            if (raw.curses != null)
            {
                foreach (var curse in raw.curses)
                {
                    if (ValidCurses.Contains(curse.id))
                        validCurseIds.Add(curse.id);
                    else
                        logger?.LogWarning($"Unknown Curse ID dropped: '{curse.id}'");
                }
            }
            model.CurseNodeIDs = validCurseIds.ToArray();

            // Clamp global modifiers (protect the physics/simulation engine)
            // Using generic clamping logic for the modifiers defined in ContractModel
            model.DamageModifier = Mathf.Clamp(raw.damageModifier, 0f, 50f);
            model.SpeedModifier = Mathf.Clamp(raw.speedModifier, 0.1f, 15f);
            model.SizeModifier = Mathf.Clamp(raw.sizeModifier, 0.1f, 10f);

            return model;
        }

        private static string CleanJsonString(string input)
        {
            string s = input.Trim();
            if (s.StartsWith("```json"))
            {
                s = s.Substring(7);
            }
            else if (s.StartsWith("```"))
            {
                s = s.Substring(3);
            }

            if (s.EndsWith("```"))
            {
                s = s.Substring(0, s.Length - 3);
            }

            return s.Trim();
        }
    }
}
