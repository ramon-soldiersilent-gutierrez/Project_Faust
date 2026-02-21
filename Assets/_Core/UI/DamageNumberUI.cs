using UnityEngine;
using Faust.Rails;
using System.Collections.Generic;

namespace Faust.UI
{
    public class DamageNumberUI : MonoBehaviour
    {
        public static DamageNumberUI Instance { get; private set; }

        private class DamageText
        {
            public float Amount;
            public Vector3 WorldPosition;
            public float Lifetime;
            public float MaxLifetime;
        }

        private List<DamageText> _activeTexts = new List<DamageText>();
        private Camera _mainCamera;

        [Header("Settings")]
        public float FloatSpeed = 2f;
        public float TextDuration = 1.0f;
        public float SpreadRadius = 0.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            CombatEventBus.OnHit += HandleHit;
            CombatEventBus.OnPlayerDamaged += HandlePlayerDamaged;
        }

        private void OnDisable()
        {
            CombatEventBus.OnHit -= HandleHit;
            CombatEventBus.OnPlayerDamaged -= HandlePlayerDamaged;
        }

        private void HandleHit(HitInfo info)
        {
            if (info.Target != null)
            {
                // Add tiny jitter so overlapping hits don't entirely stack visually
                Vector3 jitter = new Vector3(Random.Range(-SpreadRadius, SpreadRadius), 0, Random.Range(-SpreadRadius, SpreadRadius));
                SpawnDamageText(info.Context.FinalDamage, info.Target.position + Vector3.up * 2f + jitter);
            }
        }

        private void HandlePlayerDamaged(float amount)
        {
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                SpawnDamageText(amount, Faust.Simulation.PlayerController.Instance.transform.position + Vector3.up * 2f, true);
            }
        }

        private void SpawnDamageText(float amount, Vector3 worldPos, bool isPlayer = false)
        {
            _activeTexts.Add(new DamageText
            {
                Amount = amount,
                WorldPosition = worldPos,
                Lifetime = TextDuration,
                MaxLifetime = TextDuration
            });
        }

        private void Update()
        {
            // Float upwards over time
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                var dt = _activeTexts[i];
                dt.WorldPosition += Vector3.up * FloatSpeed * Time.deltaTime;
                dt.Lifetime -= Time.deltaTime;

                if (dt.Lifetime <= 0f)
                {
                    _activeTexts.RemoveAt(i);
                }
            }
        }

        private void OnGUI()
        {
            if (_mainCamera == null) return;
            // Hide if game is paused by menu manager
            if (Time.timeScale == 0f) return;

            GUIStyle damageStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24
            };

            foreach (var dt in _activeTexts)
            {
                // Translate world position to screen coordinates
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(dt.WorldPosition);

                // If z < 0, it's behind the camera
                if (screenPos.z < 0) continue;

                // IMGUI Y is inverted compared to Screen Point Y
                float guiY = Screen.height - screenPos.y;

                // Fade out based on lifetime
                float alpha = Mathf.Clamp01(dt.Lifetime / dt.MaxLifetime);
                damageStyle.normal.textColor = new Color(1f, 0.2f, 0.2f, alpha); // Red damage numbers
                
                // Draw shadow for readability
                GUIStyle shadowStyle = new GUIStyle(damageStyle);
                shadowStyle.normal.textColor = new Color(0, 0, 0, alpha);
                GUI.Label(new Rect(screenPos.x - 50 + 2, guiY - 25 + 2, 100, 50), Mathf.RoundToInt(dt.Amount).ToString(), shadowStyle);

                // Draw main text
                GUI.Label(new Rect(screenPos.x - 50, guiY - 25, 100, 50), Mathf.RoundToInt(dt.Amount).ToString(), damageStyle);
            }
        }
    }
}
