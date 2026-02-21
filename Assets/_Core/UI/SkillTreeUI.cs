using UnityEngine;
using Faust.Rails;
using System.Collections;
using System.Collections.Generic;

namespace Faust.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        [Header("IMGUI Settings")]
        public Rect TreeRect = new Rect(730, 10, 400, 400);
        public float NodeSpacing = 60f;
        public float NodeSize = 40f;

        private SkillTreeChunk _currentChunk;
        private Vector2 _scrollPosition;
        private Dictionary<string, SkillTreeNode> _nodeMap = new Dictionary<string, SkillTreeNode>();

        private bool _showSkillTree = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _showSkillTree = !_showSkillTree;
            }
        }

        public void LoadChunk(SkillTreeChunk chunk)
        {
            _currentChunk = chunk;
            _showSkillTree = true; // Auto-open when loaded
            _nodeMap.Clear();
            if (chunk != null)
            {
                foreach (var node in chunk.Nodes)
                {
                    _nodeMap[node.NodeID] = node;
                }
            }
        }

        public void Clear()
        {
            _currentChunk = null;
            _nodeMap.Clear();
            _showSkillTree = false;
        }

        private void OnGUI()
        {
            if (!_showSkillTree) return;

            if (_currentChunk == null)
            {
                GUILayout.BeginArea(TreeRect, "Faustian Tree", GUI.skin.window);
                GUILayout.Label("Skill Tree Empty.\nGenerate a tree to behold the Faustian Web.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button("Simulate Skill Tree Generation", GUILayout.Height(40)))
                {
                    SimulateGenerateTree();
                }
                GUILayout.EndArea();
                return;
            }

            GUILayout.BeginArea(TreeRect, $"Faustian Tree: {_currentChunk.ChunkName}", GUI.skin.window);
            
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
                
                // Colorize based on keystone
                GUI.backgroundColor = node.IsKeystone ? Color.red : Color.white;
                
                if (GUI.Button(nodeRect, node.NodeID, nodeStyle))
                {
                    AIConsole.Instance?.Log($"<b>{node.DisplayName}</b>\n<i>{node.FlavorText}</i>\nBoons: {node.GrantedBoonIDs?.Length ?? 0}\nCurses: {node.GrantedCurseIDs?.Length ?? 0}");
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
            yield return new WaitForSeconds(1.0f);
            
            var chunk = new SkillTreeChunk
            {
                ChunkName = "The Bloodstained Path",
                Nodes = new SkillTreeNode[]
                {
                    new SkillTreeNode { NodeID = "n0", DisplayName = "Start", GridX = 0, GridY = 0, ConnectedNodeIDs = new[] {"n1", "n2"}, FlavorText = "The beginning." },
                    new SkillTreeNode { NodeID = "n1", DisplayName = "Damage+", GridX = 1, GridY = -1, ConnectedNodeIDs = new[] {"n3"}, FlavorText = "Blood flows." },
                    new SkillTreeNode { NodeID = "n2", DisplayName = "Speed+", GridX = 1, GridY = 1, ConnectedNodeIDs = new[] {"n3"}, FlavorText = "Heart pounds." },
                    new SkillTreeNode { NodeID = "n3", DisplayName = "Toll of Blood", GridX = 2, GridY = 0, IsKeystone = true, FlavorText = "Pay the ultimate price.", GrantedBoonIDs = new[] {"Boon_DamageSpike"}, GrantedCurseIDs = new[] {"Curse_SelfDamage"} }
                }
            };
            
            AIConsole.Instance?.Log("Skill Tree Received!");
            LoadChunk(chunk);
        }
    }
}
