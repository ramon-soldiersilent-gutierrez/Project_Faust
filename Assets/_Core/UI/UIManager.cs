using UnityEngine;

namespace Faust.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
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

            Time.timeScale = anyMenuOpen ? 0f : 1f;
        }
    }
}
