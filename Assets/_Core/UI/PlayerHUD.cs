using UnityEngine;
using Faust.Simulation;

namespace Faust.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        public static PlayerHUD Instance { get; private set; }

        [Header("HP Assets")]
        public Texture2D HpBackground;     // e.g., lil_roundbackground.png
        public Texture2D HpFillTexture;    // e.g., Hp_line.png or a generated solid red texture
        public Texture2D HpOverlayFrame;   // e.g., big_roundframe.png

        [Header("Action Bar Assets")]
        public Texture2D ActionBarBackground; // e.g., mid_background.png
        public Texture2D ActionSlotFrame;     // e.g., button_frame.png
        
        [Header("XP Bar Assets")]
        public Texture2D XpFillTexture;      // Flat yellow texture or similar

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (PlayerController.Instance == null) return;
            
            // Only draw HUD if the game is active (not paused by menus)
            if (Time.timeScale == 0f) return;

            DrawHealthGlobe();
            DrawActionBar();
        }

        private void DrawHealthGlobe()
        {
            // Center bottom layout for HP globe (left side of action bar usually, but let's center-left it)
            float globeSize = 120f;
            float padding = 20f;
            Rect drawRect = new Rect(Screen.width / 2f - 300f, Screen.height - globeSize - padding, globeSize, globeSize);

            // 1. Draw Background
            if (HpBackground != null)
            {
                GUI.DrawTexture(drawRect, HpBackground);
            }

            // 2. Draw dynamic HP fill (slicing from top to bottom)
            float hpPercent = PlayerController.Instance.MaxHealth > 0 
                ? PlayerController.Instance.CurrentHealth / PlayerController.Instance.MaxHealth 
                : 0f;

            if (HpFillTexture != null && hpPercent > 0)
            {
                // Calculate height of the fill based on percentage
                float fillHeight = globeSize * hpPercent;

                // We want the fill to anchor to the BOTTOM of the globe.
                // Y increases downwards in IMGUI, so we offset the Y position by the empty space.
                float emptySpaceOffset = globeSize - fillHeight;

                Rect fillScreenRect = new Rect(
                    drawRect.x, 
                    drawRect.y + emptySpaceOffset, 
                    globeSize, 
                    fillHeight
                );

                // Calculate UVs. Unity UVs (0,0) is bottom-left. 
                // To slice from the top, we change the maximum V coord. 
                // V goes from 0 (bottom) to 1 (top). 
                // If hpPercent = 0.8, we slice off the top 0.2. So UV height is 0.8.
                Rect texCoords = new Rect(0f, 0f, 1f, hpPercent);

                GUI.DrawTextureWithTexCoords(fillScreenRect, HpFillTexture, texCoords);
            }

            // 3. Draw Overlay Frame
            if (HpOverlayFrame != null)
            {
                GUI.DrawTexture(drawRect, HpOverlayFrame);
            }
            
            // 4. Draw absolute numeric text over it
            GUIStyle txtStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            txtStyle.normal.textColor = Color.white;
            GUI.Label(drawRect, $"{Mathf.CeilToInt(PlayerController.Instance.CurrentHealth)} / {PlayerController.Instance.MaxHealth}", txtStyle);
        }

        private void DrawActionBar()
        {
            float slotSize = 60f;
            float padding = 10f;
            int numSlots = 6;
            
            float totalWidth = (slotSize * numSlots) + (padding * (numSlots - 1));
            float startX = Screen.width / 2f - 120f; // Shift to right of HP globe
            float startY = Screen.height - slotSize - 20f;

            string[] bindings = { "LMB", "RMB", "1", "2", "3", "4" };

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.white;

            for (int i = 0; i < numSlots; i++)
            {
                Rect slotRect = new Rect(startX + (i * (slotSize + padding)), startY, slotSize, slotSize);

                if (ActionSlotFrame != null)
                {
                    GUI.DrawTexture(slotRect, ActionSlotFrame);
                }
                else
                {
                    // Fallback block if no texture dragged in
                    GUI.Box(slotRect, "");
                }

                // Draw input binding text
                GUI.Label(slotRect, bindings[i], labelStyle);
            }

            // Draw XP Bar underneath
            float xpBarHeight = 8f;
            float xpBarY = startY + slotSize + 2f;
            Rect xpBarBgRect = new Rect(startX, xpBarY, totalWidth, xpBarHeight);
            
            // Background
            GUI.Box(xpBarBgRect, "");

            // Fill
            if (Faust.StatsAndHooks.LevelManager.Instance != null && XpFillTexture != null)
            {
                float xpPercent = 0f;
                if (Faust.StatsAndHooks.LevelManager.Instance.XpToNextLevel > 0)
                {
                     xpPercent = Faust.StatsAndHooks.LevelManager.Instance.CurrentXP / Faust.StatsAndHooks.LevelManager.Instance.XpToNextLevel;
                }
                
                Rect xpFillRect = new Rect(startX, xpBarY, totalWidth * Mathf.Clamp01(xpPercent), xpBarHeight);
                GUI.DrawTexture(xpFillRect, XpFillTexture);
            }
        }
    }
}
