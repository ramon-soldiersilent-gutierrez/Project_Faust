using UnityEngine;
using Faust.Rails;
using System.Collections.Generic;

namespace Faust.UI
{
    public class AIConsole : MonoBehaviour, ILogSink
    {
        public static AIConsole Instance { get; private set; }
        
        private List<string> _logEntries = new List<string>();
        private Vector2 _scrollPosition;
        private bool _showConsole = false;
        
        [Header("IMGUI Settings")]
        public Rect ConsoleRect = new Rect(10, 10, 400, 300);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                _showConsole = !_showConsole;
            }
        }

        public void Log(string message)
        {
            AppendText($"<color=white>[LOG]</color> {message}");
        }

        public void LogWarning(string message)
        {
            AppendText($"<color=yellow>[WARN]</color> {message}");
        }

        public void LogError(string message)
        {
            AppendText($"<color=red>[ERR]</color> {message}");
        }

        public void Clear()
        {
            _logEntries.Clear();
        }

        private void AppendText(string text)
        {
            _logEntries.Add(text);
            if (_logEntries.Count > 100) _logEntries.RemoveAt(0); // Cap history
            _scrollPosition.y = float.MaxValue; // Auto-scroll to bottom
            Debug.Log($"[AI_OutputConsole] {text}");
        }

        private void OnGUI()
        {
            if (!_showConsole) return;

            GUILayout.BeginArea(ConsoleRect, "AI Console", GUI.skin.window);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            // Draw log
            GUIStyle logStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            foreach (var entry in _logEntries)
            {
                GUILayout.Label(entry, logStyle);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
