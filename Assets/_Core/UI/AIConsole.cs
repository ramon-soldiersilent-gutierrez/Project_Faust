using UnityEngine;
using TMPro;
using Faust.Rails;

namespace Faust.UI
{
    public class AIConsole : MonoBehaviour, ILogSink
    {
        public static AIConsole Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private TMP_Text outputText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
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
            if (outputText != null)
                outputText.text = "";
        }

        private void AppendText(string text)
        {
            if (outputText != null)
            {
                outputText.text += text + "\n\n";
                // Auto-scroll could be handled here if attached to a ScrollRect
            }
            Debug.Log($"[AI_OutputConsole] {text}");
        }
    }
}
