using System;
using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.AI
{
    [Serializable]
    public class RawSkillTreeNode
    {
        public string nodeID;
        public string displayName;
        public string flavorText;
        public string[] connectedNodeIDs;
        public int gridX;
        public int gridY;
        public bool isKeystone;
        public RawContractNode[] grantedBoons;
        public RawContractNode[] grantedCurses;
        public float damageDelta;
        public float speedDelta;
        public float sizeDelta;
    }

    [Serializable]
    public class RawSkillTreeChunk
    {
        public string chunkName;
        public string theme;
        public RawSkillTreeNode[] nodes;
    }

    public static class SkillTreeParser
    {
        private static readonly HashSet<string> ValidBoons = new HashSet<string>
        {
            "Boon_DamageSpike", "Boon_MachineGun", "Boon_Multicast", "Boon_Vampiric"
        };

        private static readonly HashSet<string> ValidCurses = new HashSet<string>
        {
            "Curse_TeleportOnHit", "Curse_GlassCannon", "Curse_SelfDamage", "Curse_Rooted"
        };

        public static SkillTreeChunk ParseAndValidate(string json, ILogSink logger = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Skill Tree JSON is null or empty.");
            }

            string cleanJson = CleanJsonString(json);
            RawSkillTreeChunk raw = JsonUtility.FromJson<RawSkillTreeChunk>(cleanJson);
            
            if (raw == null || raw.nodes == null)
            {
                throw new Exception("Failed to parse Skill Tree JSON into expected shape.");
            }

            SkillTreeChunk chunk = new SkillTreeChunk
            {
                ChunkName = string.IsNullOrEmpty(raw.chunkName) ? "The Nameless Web" : raw.chunkName,
                Theme = string.IsNullOrEmpty(raw.theme) ? "Unknown Theme" : raw.theme,
                Nodes = new SkillTreeNode[raw.nodes.Length]
            };

            for (int i = 0; i < raw.nodes.Length; i++)
            {
                var rawNode = raw.nodes[i];
                var validatedNode = new SkillTreeNode
                {
                    NodeID = rawNode.nodeID,
                    DisplayName = rawNode.displayName,
                    FlavorText = rawNode.flavorText,
                    ConnectedNodeIDs = rawNode.connectedNodeIDs ?? new string[0],
                    GridX = rawNode.gridX,
                    GridY = rawNode.gridY,
                    IsKeystone = rawNode.isKeystone,
                    
                    // Stat Clamping (Extremely restrictive to avoid game breaks on a nodal scale)
                    DamageDelta = Mathf.Clamp(rawNode.damageDelta, -0.5f, 0.5f), // Max 50% change per node
                    SpeedDelta = Mathf.Clamp(rawNode.speedDelta, -0.5f, 0.5f),
                    SizeDelta = Mathf.Clamp(rawNode.sizeDelta, -0.2f, 0.2f)
                };

                // Validate Keystone Grants
                if (validatedNode.IsKeystone)
                {
                    validatedNode.GrantedBoonIDs = ValidateNodes(rawNode.grantedBoons, ValidBoons, logger, "Boon");
                    validatedNode.GrantedCurseIDs = ValidateNodes(rawNode.grantedCurses, ValidCurses, logger, "Curse");
                }
                else
                {
                    validatedNode.GrantedBoonIDs = new string[0];
                    validatedNode.GrantedCurseIDs = new string[0];
                }

                chunk.Nodes[i] = validatedNode;
            }

            return chunk;
        }

        private static string[] ValidateNodes(RawContractNode[] rawNodes, HashSet<string> validSet, ILogSink logger, string typeName)
        {
            List<string> validIds = new List<string>();
            if (rawNodes != null)
            {
                foreach (var node in rawNodes)
                {
                    if (validSet.Contains(node.id))
                        validIds.Add(node.id);
                    else
                        logger?.LogWarning($"Unknown SkillTree {typeName} ID dropped: '{node.id}'");
                }
            }
            return validIds.ToArray();
        }

        private static string CleanJsonString(string input)
        {
            string s = input.Trim();
            if (s.StartsWith("```json")) s = s.Substring(7);
            else if (s.StartsWith("```")) s = s.Substring(3);
            if (s.EndsWith("```")) s = s.Substring(0, s.Length - 3);
            return s.Trim();
        }
    }
}
