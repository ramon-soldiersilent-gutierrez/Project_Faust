using System;

namespace Faust.Rails
{
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
        public float DamageDelta;
        public float SpeedDelta;
        public float SizeDelta;
    }
}
