using UnityEngine;
using Faust.Rails;
using System.Collections;
using System.Collections.Generic;

namespace Faust.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        // IMGUI Settings
        private Rect GetTreeRect() => new Rect(Screen.width - 410f, 50f, 400, 400);
        public float NodeSpacing = 60f;
        public float NodeSize = 40f;

        private SkillTreeChunk _currentChunk;
        private Vector2 _scrollPosition;
        private Dictionary<string, SkillTreeNode> _nodeMap = new Dictionary<string, SkillTreeNode>();

        public bool IsVisible { get; set; } = false;

        private void Awake()
        {
            Instance = this;
        }

        public void LoadChunk(SkillTreeChunk chunk)
        {
            _currentChunk = chunk;
            _nodeMap.Clear();
            if (chunk != null)
            {
                foreach (var node in chunk.Nodes)
                {
                    _nodeMap[node.NodeID] = node;
                }
            }
            
            // Force open the UI and close others when the AI finishes computing the tree
            if (UIManager.Instance != null) UIManager.Instance.CloseAllMenus();
            IsVisible = true;
        }

        public void Clear()
        {
            _currentChunk = null;
            _nodeMap.Clear();
            IsVisible = false;
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            
            if (Faust.StatsAndHooks.LevelManager.Instance != null)
            {
                int points = Faust.StatsAndHooks.LevelManager.Instance.AvailableSkillPoints;
                headerStyle.normal.textColor = points > 0 ? Color.yellow : Color.white;
                GUILayout.Label($"Available Skill Points: {points}", headerStyle);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset Tree", GUILayout.Width(100)))
            {
                if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
                {
                    Faust.StatsAndHooks.HookLifecycleManager.Instance.ResetTreeAllocations();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void OnGUI()
        {
            if (!IsVisible) return;

            if (_currentChunk == null)
            {
                GUILayout.BeginArea(GetTreeRect(), "Faustian Tree", GUI.skin.window);
                GUILayout.Label("Skill Tree Empty.\nGenerate a tree to behold the Faustian Web.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button("Simulate Skill Tree Generation", GUILayout.Height(40)))
                {
                    SimulateGenerateTree();
                }
                GUILayout.EndArea();
                return;
            }

            GUILayout.BeginArea(GetTreeRect(), $"Faustian Tree: {_currentChunk.ChunkName}", GUI.skin.window);
            
            DrawHeader();
            
            // Basic layout math for internal scroll view size
            float minX = 0, minY = 0, maxX = 0, maxY = 0;
            foreach (var node in _currentChunk.Nodes)
            {
                float x = node.GridX * NodeSpacing;
                float y = node.GridY * NodeSpacing;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            
            Rect viewRect = new Rect(0, 0, (maxX - minX) + NodeSize * 3, (maxY - minY) + NodeSize * 3);
            _scrollPosition = GUI.BeginScrollView(new Rect(10, 20, 380, 370), _scrollPosition, viewRect);

            // Shift origin to avoid negative coordinates cutting off nodes
            float offsetX = -minX + NodeSize;
            float offsetY = -minY + NodeSize;

            // Draw Connection Lines First
            foreach (var node in _currentChunk.Nodes)
            {
                if (node.ConnectedNodeIDs == null) continue;

                Vector2 startPos = new Vector2(node.GridX * NodeSpacing + offsetX + NodeSize/2, node.GridY * NodeSpacing + offsetY + NodeSize/2);
                
                foreach (var targetId in node.ConnectedNodeIDs)
                {
                    if (_nodeMap.TryGetValue(targetId, out var targetNode))
                    {
                        Vector2 endPos = new Vector2(targetNode.GridX * NodeSpacing + offsetX + NodeSize/2, targetNode.GridY * NodeSpacing + offsetY + NodeSize/2);
                        DrawLine(startPos, endPos, Color.grey, 2f);
                    }
                }
            }

            // Draw Nodes
            GUIStyle nodeStyle = new GUIStyle(GUI.skin.button) { wordWrap = true, fontSize = 10 };
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };
            
            foreach (var node in _currentChunk.Nodes)
            {
                Rect nodeRect = new Rect(node.GridX * NodeSpacing + offsetX, node.GridY * NodeSpacing + offsetY, NodeSize, NodeSize);
                
                int currentPoints = Faust.StatsAndHooks.LevelManager.Instance != null ? Faust.StatsAndHooks.LevelManager.Instance.AvailableSkillPoints : 0;
                bool canAfford = currentPoints > 0;
                
                bool isAllocated = false;
                if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
                {
                    isAllocated = Faust.StatsAndHooks.HookLifecycleManager.Instance.AllocatedNodeIDs.Contains(node.NodeID);
                }

                // Colorize based on allocation or keystone
                if (isAllocated)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else if (node.IsKeystone)
                {
                    GUI.backgroundColor = Color.red;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
                
                if (GUI.Button(nodeRect, node.NodeID, nodeStyle))
                {
                    if (!isAllocated && canAfford)
                    {
                        if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
                        {
                            Faust.StatsAndHooks.HookLifecycleManager.Instance.ToggleNodeAllocation(node);
                        }
                    }
                        
                    // Flavor text logging
                    int boonCount = node.GrantedBoonIDs != null ? node.GrantedBoonIDs.Length : 0;
                    int curseCount = node.GrantedCurseIDs != null ? node.GrantedCurseIDs.Length : 0;
                    AIConsole.Instance?.Log($"[{node.DisplayName}] {node.FlavorText} (Boons: {boonCount}, Curses: {curseCount})");
                }
                
                GUI.backgroundColor = Color.white;
                
                // Label above node
                Rect labelRect = new Rect(nodeRect.x - 20, nodeRect.y - 15, NodeSize + 40, 20);
                GUI.Label(labelRect, node.DisplayName, headerStyle);
            }

            GUI.EndScrollView();
            GUILayout.EndArea();
        }

        // Simple IMGUI Line drawing
        private Texture2D _lineTex;
        private void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            if (_lineTex == null)
            {
                _lineTex = new Texture2D(1, 1);
            }
            Color guiColor = GUI.color;
            GUI.color = color;
            float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
            float distance = Vector2.Distance(pointA, pointB);
            
            GUIUtility.RotateAroundPivot(angle, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, distance, width), _lineTex);
            GUIUtility.RotateAroundPivot(-angle, pointA);
            GUI.color = guiColor;
        }

        // --- Demo Test Coroutine ---
        public void SimulateGenerateTree()
        {
            AIConsole.Instance?.Log("Simulating Skill Tree Generation...");
            StartCoroutine(GenerateMockTree());
        }

        private IEnumerator GenerateMockTree()
        {
            var chunk = new SkillTreeChunk
            {
                ChunkName = "The Bloodstained Path",
                Nodes = new SkillTreeNode[]
                {
                    new SkillTreeNode { NodeID = "n0", DisplayName = "Start", GridX = 0, GridY = 0, ConnectedNodeIDs = new[] {"n1", "n2"}, FlavorText = "The beginning." },
                    new SkillTreeNode { NodeID = "n1", DisplayName = "Damage+", GridX = 1, GridY = -1, ConnectedNodeIDs = new[] {"n3"}, FlavorText = "Blood flows.", DamageDelta = 0.5f }, // +50% Damage
                    new SkillTreeNode { NodeID = "n2", DisplayName = "Speed+", GridX = 1, GridY = 1, ConnectedNodeIDs = new[] {"n3"}, FlavorText = "Heart pounds.", SpeedDelta = 0.5f },   // +50% Speed
                    new SkillTreeNode { NodeID = "n3", DisplayName = "Toll of Blood", GridX = 2, GridY = 0, IsKeystone = true, FlavorText = "Pay the ultimate price.", GrantedBoonIDs = new[] {"Boon_DamageSpike", "Boon_Multicast"}, GrantedCurseIDs = new[] {"Curse_SelfDamage"} }
                }
            };
            
            AIConsole.Instance?.Log("Skill Tree Received!");
            LoadChunk(chunk);
            yield break;
        }
    }
}
