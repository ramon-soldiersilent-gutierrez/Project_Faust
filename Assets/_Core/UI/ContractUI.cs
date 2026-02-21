using UnityEngine;
using Faust.Rails;
using System.Collections;
using System.Collections.Generic;

namespace Faust.UI
{
    public class ContractUI : MonoBehaviour
    {
        public static ContractUI Instance { get; private set; }

        // IMGUI Settings
        private Rect GetUIRect() => new Rect(Screen.width / 2f - 150f, 10, 300, 200);
        private Rect GetCharacterSheetRect() => new Rect(10f, 100f, 600, 500);

        private string _wishText = "I want infinite power";
        private float _greedValue = 50f;
        private bool _isForging = false;

        // Inventory State
        private ContractModel _weaponSlot;
        private ContractModel _armorSlot;
        private ContractModel _accessorySlot;
        
        // JRPG Stash State
        public List<ContractModel> PlayerStash = new List<ContractModel>();
        private string _activeDropdownSlot = ""; // "Weapon", "Armor", "Accessory" or ""

        // UI State
        public bool IsForgeVisible { get; set; } = false;
        public bool IsInventoryVisible { get; set; } = false;
        private ContractModel _hoveredItem;

        private void Awake()
        {
            Instance = this;
        }

        private void OnGUI()
        {
            _hoveredItem = null; // Reset every frame
            DrawHelperText();
            if (IsForgeVisible) DrawForgeWindow();
            if (IsInventoryVisible) DrawCharacterSheetWindow();

            if (_hoveredItem != null && IsInventoryVisible)
            {
                DrawItemTooltip(_hoveredItem);
            }
        }

        private void DrawHelperText()
        {
            GUIStyle helperStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            helperStyle.normal.textColor = Color.yellow;

            Rect helperRect = new Rect(Screen.width - 350, 10, 340, 30);
            GUI.Label(helperRect, "F: Forge | I: Character Sheet | T: Skill Tree | C: Console", helperStyle);
        }

        private void DrawForgeWindow()
        {
            GUILayout.BeginArea(GetUIRect(), "Faustian Forge", GUI.skin.window);

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

        private void DrawCharacterSheetWindow()
        {
            GUILayout.BeginArea(GetCharacterSheetRect(), "Character Sheet", GUI.skin.window);
            GUIStyle richTextLabel = new GUIStyle(GUI.skin.label) { richText = true };
            
            GUILayout.BeginHorizontal();
            
            // --- LEFT COLUMN: STATS ---
            GUILayout.BeginVertical(GUILayout.Width(280));
            GUILayout.Label("<color=cyan><b>Core Statistics</b></color>", richTextLabel);
            
            if (Faust.Simulation.PlayerController.Instance != null && Faust.StatsAndHooks.LevelManager.Instance != null)
            {
                GUILayout.Label($"Level: {Faust.StatsAndHooks.LevelManager.Instance.CurrentLevel}");
                GUILayout.Label($"Max HP: {Faust.Simulation.PlayerController.Instance.MaxHealth}");
                GUILayout.Label($"Base Damage: {Faust.Simulation.PlayerController.Instance.BaseDamage:F1}");
                GUILayout.Label($"Proj Speed: {Faust.Simulation.PlayerController.Instance.BaseProjectileSpeed:F1}");
            }
            else
            {
                GUILayout.Label("Simulation Offline.", richTextLabel);
            }

            GUILayout.Space(10);
            GUILayout.Label("<color=cyan><b>Multipliers</b></color>", richTextLabel);
            if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
            {
                GUILayout.Label($"Damage: x{Faust.StatsAndHooks.HookLifecycleManager.Instance.CurrentDamageMultiplier:F2}");
                GUILayout.Label($"Speed: x{Faust.StatsAndHooks.HookLifecycleManager.Instance.CurrentSpeedMultiplier:F2}");
                
                GUILayout.Space(10);
                GUILayout.Label("<color=cyan><b>Active Boons</b></color>", richTextLabel);
                
                var activeBoons = Faust.StatsAndHooks.HookLifecycleManager.Instance.GetActiveBoons();
                if (activeBoons.Count == 0)
                {
                    GUILayout.Label("<color=gray>None</color>", richTextLabel);
                }
                else
                {
                    foreach (var boon in activeBoons)
                    {
                        GUILayout.Label($"• {boon}");
                    }
                }
            }
            GUILayout.EndVertical();

            // Divider
            GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
            GUILayout.Space(10);

            // --- RIGHT COLUMN: EQUIPPED ITEMS ---
            GUILayout.BeginVertical(GUILayout.Width(280));
            GUILayout.Label("<color=cyan><b>Equipped Items</b></color>", richTextLabel);
            GUILayout.Space(5);

            DrawPoEItemSlot("Weapon", ref _weaponSlot, richTextLabel);
            if (_activeDropdownSlot == "Weapon") DrawDropdown("Weapon", ref _weaponSlot, richTextLabel);

            GUILayout.Space(5);
            DrawPoEItemSlot("Armor", ref _armorSlot, richTextLabel);
            if (_activeDropdownSlot == "Armor") DrawDropdown("Armor", ref _armorSlot, richTextLabel);

            GUILayout.Space(5);
            DrawPoEItemSlot("Accessory", ref _accessorySlot, richTextLabel);
            if (_activeDropdownSlot == "Accessory") DrawDropdown("Accessory", ref _accessorySlot, richTextLabel);

            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawPoEItemSlot(string slotName, ref ContractModel model, GUIStyle style)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{slotName}</b>", GUILayout.Width(70));
            if (model == null)
            {
                GUILayout.Label("<color=gray>Empty</color>", style);
            }
            else
            {
                // RARE ITEM HEADER STYLE (Yellow/Gold color typical in PoE)
                GUILayout.Label($"<color=#FFD700><b>{model.ItemName}</b></color>", style);
            }
            if (GUILayout.Button("Eqp", GUILayout.Width(40)))
            {
                _activeDropdownSlot = _activeDropdownSlot == slotName ? "" : slotName;
            }
            GUILayout.EndHorizontal();

            if (model != null)
            {
                // Item Base Type and Flavor
                GUILayout.Label($"<color=#888888><i>{model.SpriteKeyword.Replace('_', ' ')}</i></color>", style);
            }
            
            GUILayout.EndVertical();
            
            if (model != null && Event.current.type == EventType.Repaint)
            {
                Rect slotRect = GUILayoutUtility.GetLastRect();
                if (slotRect.Contains(Event.current.mousePosition))
                {
                    _hoveredItem = model;
                }
            }
        }

        private void DrawDropdown(string category, ref ContractModel equippedSlot, GUIStyle style)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<i>Available {category}s:</i>", style);
            
            bool hasItems = false;
            for (int i = 0; i < PlayerStash.Count; i++)
            {
                var item = PlayerStash[i];
                if (item.EquipSlot == category)
                {
                    hasItems = true;
                    // Yellow name for listing
                    if (GUILayout.Button($"<color=#FFD700>{item.ItemName}</color>", style))
                    {
                        // Swap
                        if (equippedSlot != null) PlayerStash.Add(equippedSlot);
                        equippedSlot = item;
                        PlayerStash.RemoveAt(i);
                        
                        _activeDropdownSlot = ""; // Close
                        if (Faust.StatsAndHooks.HookLifecycleManager.Instance != null)
                            Faust.StatsAndHooks.HookLifecycleManager.Instance.EquipItem(item);
                        break;
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            _hoveredItem = item;
                        }
                    }
                }
            }
            
            if (!hasItems) GUILayout.Label("<color=gray>None in stash</color>", style);
            
            GUILayout.EndVertical();
        }

        private void DrawItemTooltip(ContractModel model)
        {
            Vector2 mousePos = Event.current.mousePosition;
            float tooltipWidth = 250f;
            float tooltipHeight = 150f; // flexible
            
            Rect tooltipRect = new Rect(mousePos.x + 15, mousePos.y + 15, tooltipWidth, tooltipHeight);
            
            // Keep on screen bounds
            if (tooltipRect.xMax > Screen.width) tooltipRect.x -= (tooltipWidth + 30);
            if (tooltipRect.yMax > Screen.height) tooltipRect.y -= (tooltipHeight + 30);

            GUILayout.BeginArea(tooltipRect, GUI.skin.box);
            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = 12 };

            GUILayout.Label($"<color=#FFD700><b>{model.ItemName}</b></color>", style);
            GUILayout.Label($"<color=#888888><i>{model.SpriteKeyword.Replace('_', ' ')}</i></color>", style);
            GUILayout.Label("<color=#555555>--------</color>", style);

            if (model.DamageModifier != 1.0f)
            {
                float pct = (model.DamageModifier - 1f) * 100f;
                string sign = pct > 0 ? "+" : "";
                GUILayout.Label($"<color=#8888FF>{sign}{pct:F1}% to Global Damage</color>", style);
            }
            if (model.SpeedModifier != 1.0f)
            {
                float pct = (model.SpeedModifier - 1f) * 100f;
                string sign = pct > 0 ? "+" : "";
                GUILayout.Label($"<color=#8888FF>{sign}{pct:F1}% increased Action Speed</color>", style);
            }
            if (model.SizeModifier != 1.0f)
            {
                float pct = (model.SizeModifier - 1f) * 100f;
                string sign = pct > 0 ? "+" : "";
                GUILayout.Label($"<color=#8888FF>{sign}{pct:F1}% increased Area of Effect</color>", style);
            }

            if (!string.IsNullOrEmpty(model.GrantedSkillID))
            {
                GUILayout.Label($"<color=#FFFFFF>Grants Skill: {model.GrantedSkillID}</color>", style);
            }

            if (model.BoonNodeIDs != null && model.BoonNodeIDs.Length > 0)
            {
                GUILayout.Label($"<color=#00FF00>Has Active Boon: {model.BoonNodeIDs[0]}</color>", style);
            }

            if (model.CurseNodeIDs != null && model.CurseNodeIDs.Length > 0)
            {
                GUILayout.Label($"<color=#FF0000>Cursed with: {model.CurseNodeIDs[0]}</color>", style);
            }

            GUILayout.Label("<color=#555555>--------</color>", style);
            GUILayout.Label($"<color=#AF6025><i>\"{model.FlavorText}\"</i></color>", style);
            
            GUILayout.EndArea();
        }

        public void AddStashItem(ContractModel model)
        {
            PlayerStash.Add(model);
        }

        public void ClearInventory()
        {
            _weaponSlot = null;
            _armorSlot = null;
            _accessorySlot = null;
            PlayerStash.Clear();
            _activeDropdownSlot = "";
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
