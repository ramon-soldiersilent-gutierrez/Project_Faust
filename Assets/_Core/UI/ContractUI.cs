using UnityEngine;
using Faust.Rails;
using System.Collections;

namespace Faust.UI
{
    public class ContractUI : MonoBehaviour
    {
        public static ContractUI Instance { get; private set; }

        [Header("IMGUI Settings")]
        public Rect UIRect = new Rect(420, 10, 300, 200);
        public Rect InventoryRect = new Rect(420, 220, 300, 120);

        private string _wishText = "I want infinite power";
        private float _greedValue = 50f;
        private bool _isForging = false;

        // Inventory State
        private ContractModel _weaponSlot;
        private ContractModel _armorSlot;
        private ContractModel _accessorySlot;

        // UI State
        private bool _showForge = true;
        private bool _showInventory = true;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F)) _showForge = !_showForge;
            if (Input.GetKeyDown(KeyCode.I)) _showInventory = !_showInventory;
        }

        private void OnGUI()
        {
            if (_showForge) DrawForgeWindow();
            if (_showInventory) DrawInventoryWindow();
        }

        private void DrawForgeWindow()
        {
            GUILayout.BeginArea(UIRect, "Faustian Forge", GUI.skin.window);

            GUILayout.Label("Wish:");
            _wishText = GUILayout.TextField(_wishText);

            GUILayout.Space(10);
            GUILayout.Label($"Greed Level: {Mathf.RoundToInt(_greedValue)}");
            _greedValue = GUILayout.HorizontalSlider(_greedValue, 0f, 100f);

            GUILayout.Space(20);

            GUI.enabled = !_isForging; // Disable button while forging
            if (GUILayout.Button("FORGE ITEM", GUILayout.Height(40)))
            {
                OnForgeButtonClicked();
            }
            GUI.enabled = true;

            GUILayout.EndArea();
        }

        private void DrawInventoryWindow()
        {
            GUILayout.BeginArea(InventoryRect, "Equipped Contracts", GUI.skin.window);
            
            GUIStyle richTextLabel = new GUIStyle(GUI.skin.label) { richText = true };
            
            DrawSlot("Weapon", _weaponSlot, richTextLabel);
            DrawSlot("Armor", _armorSlot, richTextLabel);
            DrawSlot("Accessory", _accessorySlot, richTextLabel);
            
            GUILayout.EndArea();
        }

        private void DrawSlot(string slotName, ContractModel model, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(slotName, GUILayout.Width(80));
            if (model == null)
            {
                GUILayout.Label("<color=gray>Empty</color>", style);
            }
            else
            {
                GUILayout.Label($"<color=green>{model.ItemName}</color> ({model.SpriteKeyword})", style);
            }
            GUILayout.EndHorizontal();
        }

        public void ClearInventory()
        {
            _weaponSlot = null;
            _armorSlot = null;
            _accessorySlot = null;
        }

        private void OnForgeButtonClicked()
        {
            _isForging = true;

            int greedTier = 0;
            if (_greedValue <= 30) greedTier = 1;
            else if (_greedValue <= 70) greedTier = 2;
            else greedTier = 3;

            AIConsole.Instance?.Log($"Requesting Contract: Wish='{_wishText}', Greed={_greedValue} (Tier {greedTier})");

            if (Faust.AI.AIPipeline.Instance != null)
            {
                Faust.AI.AIPipeline.Instance.RequestContract(_wishText, _greedValue, OnContractReceived);
            }
            else
            {
                Debug.LogWarning("ContractUI: AIPipeline Instance not found! Using fallback simulator.");
                StartCoroutine(SimulateAIResponse(_wishText, greedTier));
            }
        }

        private IEnumerator SimulateAIResponse(string wish, int greedTier)
        {
            yield return new WaitForSeconds(1.5f);
            
            // Randomly select an equip slot if simulating
            string[] slots = new string[] { "Weapon", "Armor", "Accessory" };
            string chosenSlot = slots[Random.Range(0, slots.Length)];

            var stubModel = new ContractModel
            {
                ItemName = $"Stub {chosenSlot}",
                FlavorText = "A placeholder answer.",
                EquipSlot = chosenSlot,
                SpriteKeyword = $"{chosenSlot}_Generic",
                GrantedSkillID = "Kinetic_Projectile",
                BoonNodeIDs = new string[] { "Boon_DamageSpike" },
                CurseNodeIDs = new string[] { "Curse_GlassCannon" }
            };

            OnContractReceived(stubModel);
        }

        private void OnContractReceived(ContractModel model)
        {
            AIConsole.Instance?.Log($"Received Contract:\n{JsonUtility.ToJson(model, true)}");
            _isForging = false;

            // Slot it in the inventory
            if (model.EquipSlot == "Weapon") _weaponSlot = model;
            else if (model.EquipSlot == "Armor") _armorSlot = model;
            else if (model.EquipSlot == "Accessory") _accessorySlot = model;
            else _weaponSlot = model; // fallback

            // Apply the contract via the global runtime
            if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
            {
                Faust.StatsAndHooks.HookLifecycleManager.Instance.ApplyContract(model);
            }
            else
            {
                Debug.LogError("ContractUI: HookLifecycleManager not found, could not apply contract!");
            }
        }
        
        private void OnContractFailed(string errorMsg)
        {
             AIConsole.Instance?.LogError($"Contract Failed: {errorMsg}");
             _isForging = false;
        }
    }
}
