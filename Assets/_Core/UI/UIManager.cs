using UnityEngine;

namespace Faust.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public bool IsGameOverVisible { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (IsGameOverVisible) return; // Block hotkeys when dead
            
            HandleHotkeys();
            UpdatePauseState();
        }

        private void HandleHotkeys()
        {
            // Toggles
            if (Input.GetKeyDown(KeyCode.F)) ToggleForge();
            if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
            if (Input.GetKeyDown(KeyCode.T)) ToggleSkillTree();
        }

        public void ToggleForge()
        {
            if (ContractUI.Instance != null)
            {
                bool willOpen = !ContractUI.Instance.IsForgeVisible;
                if (willOpen) CloseAllMenus();
                ContractUI.Instance.IsForgeVisible = willOpen;
            }
        }

        public void ToggleInventory()
        {
            if (ContractUI.Instance != null)
            {
                bool willOpen = !ContractUI.Instance.IsInventoryVisible;
                if (willOpen) CloseAllMenus();
                ContractUI.Instance.IsInventoryVisible = willOpen;
            }
        }

        public void ToggleSkillTree()
        {
            if (SkillTreeUI.Instance != null)
            {
                bool willOpen = !SkillTreeUI.Instance.IsVisible;
                if (willOpen) CloseAllMenus();
                SkillTreeUI.Instance.IsVisible = willOpen;
            }
        }

        public void CloseAllMenus()
        {
            if (ContractUI.Instance != null)
            {
                ContractUI.Instance.IsForgeVisible = false;
                ContractUI.Instance.IsInventoryVisible = false;
            }

            if (SkillTreeUI.Instance != null)
            {
                SkillTreeUI.Instance.IsVisible = false;
            }
        }

        public void ShowGameOver()
        {
            IsGameOverVisible = true;
            CloseAllMenus(); // Force close other menus
            Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (IsGameOverVisible)
            {
                Rect gameOverRect = new Rect(Screen.width / 2f - 150f, Screen.height / 2f - 100f, 300, 200);
                GUILayout.BeginArea(gameOverRect, "Game Over", GUI.skin.window);
                
                GUILayout.Space(20);
                
                GUIStyle deathStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24,
                    fontStyle = FontStyle.Bold
                };
                deathStyle.normal.textColor = Color.red;

                GUILayout.Label("YOU DIED", deathStyle);
                GUILayout.Space(30);

                if (GUILayout.Button("Restart Simulation", GUILayout.Height(50)))
                {
                    IsGameOverVisible = false;
                    
                    // Reset singletons properly
                    if (Faust.Simulation.SimulationManager.Instance != null) 
                        Faust.Simulation.SimulationManager.Instance.ResetAll();
                        
                    if (Faust.Simulation.PlayerController.Instance != null) 
                        Faust.Simulation.PlayerController.Instance.ResetPlayer();
                        
                    UpdatePauseState();
                }

                GUILayout.EndArea();
            }
        }

        private void UpdatePauseState()
        {
            bool anyMenuOpen = false;

            if (ContractUI.Instance != null && (ContractUI.Instance.IsForgeVisible || ContractUI.Instance.IsInventoryVisible))
            {
                anyMenuOpen = true;
            }
            if (SkillTreeUI.Instance != null && SkillTreeUI.Instance.IsVisible)
            {
                anyMenuOpen = true;
            }
            if (IsGameOverVisible)
            {
                anyMenuOpen = true;
            }

            Time.timeScale = anyMenuOpen ? 0f : 1f;
        }
    }
}
