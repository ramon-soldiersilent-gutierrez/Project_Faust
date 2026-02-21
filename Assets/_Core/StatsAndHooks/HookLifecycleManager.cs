using System;
using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.StatsAndHooks
{
    public class HookLifecycleManager : MonoBehaviour, IContractRuntime
    {
        public static HookLifecycleManager Instance { get; private set; }

        // The active list of injected gameplay logic behavior hooks
        private List<IHookInstance> _activeHooks = new List<IHookInstance>();

        public List<ContractModel> EquippedItems = new List<ContractModel>();
        public List<SkillTreeNode> ActiveTreeNodes = new List<SkillTreeNode>();
        public HashSet<string> AllocatedNodeIDs = new HashSet<string>();

        // Public getters for UI display on the Character Sheet
        public float CurrentDamageMultiplier { get; private set; } = 1.0f;
        public float CurrentSpeedMultiplier { get; private set; } = 1.0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // IContractRuntime implementation (Maintained for legacy DemoAPI calls)
        public void ApplyContract(ContractModel model)
        {
            EquipItem(model);
        }

        public void EquipItem(ContractModel model)
        {
            // If the item has a slot, remove the existing item in that slot
            if (!string.IsNullOrEmpty(model.EquipSlot))
            {
                EquippedItems.RemoveAll(i => i.EquipSlot == model.EquipSlot);
            }
            EquippedItems.Add(model);
            RefreshAll();
        }

        public void UnequipItem(ContractModel model)
        {
            if (EquippedItems.Contains(model))
            {
                EquippedItems.Remove(model);
                RefreshAll();
            }
        }

        public void ToggleNodeAllocation(SkillTreeNode node)
        {
            if (AllocatedNodeIDs.Contains(node.NodeID))
            {
                // Can't refund points yet based on requirements, but maybe in future. Ignore click for now.
                Debug.Log($"Node {node.NodeID} already allocated.");
                return;
            }

            if (LevelManager.Instance != null && LevelManager.Instance.SpendSkillPoints(1))
            {
                AllocatedNodeIDs.Add(node.NodeID);
                ActiveTreeNodes.Add(node);
                RefreshAll();
                Debug.Log($"Allocated Node {node.NodeID}. Points remaining: {LevelManager.Instance.AvailableSkillPoints}");
            }
            else
            {
                Debug.Log($"Not enough Skill Points to allocate {node.NodeID}.");
            }
        }

        public void ResetTreeAllocations()
        {
            if (LevelManager.Instance != null)
            {
                // Refund 1 point for every allocated node
                LevelManager.Instance.RefundSkillPoints(AllocatedNodeIDs.Count);
            }
            AllocatedNodeIDs.Clear();
            ActiveTreeNodes.Clear();
            RefreshAll();
            Debug.Log("Skill Tree allocations reset and points refunded.");
        }

        public List<string> GetActiveBoons()
        {
            List<string> boons = new List<string>();
            foreach (var item in EquippedItems)
            {
                if (item.BoonNodeIDs != null) boons.AddRange(item.BoonNodeIDs);
            }
            foreach (var node in ActiveTreeNodes)
            {
                if (node.GrantedBoonIDs != null) boons.AddRange(node.GrantedBoonIDs);
            }
            return boons;
        }

        public void RefreshAll()
        {
            ClearAllHooks();

            float totalDamageMultiplier = 1.0f;
            float totalSpeedMultiplier = 1.0f;

            // 1. Process Equipped Items
            foreach (var item in EquippedItems)
            {
                if (item.BoonNodeIDs != null)
                {
                    foreach (var id in item.BoonNodeIDs) { InjectHook(id); }
                }
                if (item.CurseNodeIDs != null)
                {
                    foreach (var id in item.CurseNodeIDs) { InjectHook(id); }
                }

                // Treat Item Modifiers as "More" multipliers
                totalDamageMultiplier *= item.DamageModifier;
                totalSpeedMultiplier *= item.SpeedModifier;
            }

            // 2. Process Skill Tree Nodes
            float treeDamageIncreased = 0f;
            float treeSpeedIncreased = 0f;

            foreach (var node in ActiveTreeNodes)
            {
                if (node.GrantedBoonIDs != null)
                {
                    foreach (var id in node.GrantedBoonIDs) { InjectHook(id); }
                }
                if (node.GrantedCurseIDs != null)
                {
                    foreach (var id in node.GrantedCurseIDs) { InjectHook(id); }
                }

                // Treat Tree Deltas as "Increased" additive modifiers
                treeDamageIncreased += node.DamageDelta;
                treeSpeedIncreased += node.SpeedDelta;
            }

            // 3. Compile Global Stats to PlayerController (Simulation reads from this)
            CurrentDamageMultiplier = totalDamageMultiplier * (1.0f + treeDamageIncreased);
            CurrentSpeedMultiplier = totalSpeedMultiplier * (1.0f + treeSpeedIncreased);

            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Damage = Base(10) * (1 + Sum(Increased)) * Product(More)
                Faust.Simulation.PlayerController.Instance.BaseDamage = 10f * CurrentDamageMultiplier;
                Faust.Simulation.PlayerController.Instance.BaseProjectileSpeed = 20f * CurrentSpeedMultiplier;
            }
            
            Debug.Log($"Refreshed All Stats/Hooks. Items: {EquippedItems.Count}, Nodes: {ActiveTreeNodes.Count}");
        }

        private void InjectHook(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var hook = HookRegistry.InstantiateHook(id);
            if (hook != null)
            {
                hook.Enable();
                _activeHooks.Add(hook);
            }
        }

        public void ClearAllHooks()
        {
            foreach (var hook in _activeHooks)
            {
                hook.Disable();
            }
            _activeHooks.Clear();
        }
    }

    public static class HookRegistry
    {
        // Simple map of ID -> Factory Function
        public static IHookInstance InstantiateHook(string id)
        {
            switch (id)
            {
                // Boons
                case "Boon_DamageSpike": return new DamageSpikeHook();
                case "Boon_MachineGun": return new MachineGunHook();
                case "Boon_Multicast": return new MulticastHook();
                case "Boon_Vampiric": return new VampiricHook();
                
                // Curses
                case "Curse_TeleportOnHit": return new TeleportOnHitHook();
                case "Curse_GlassCannon": return new GlassCannonHook();
                case "Curse_SelfDamage": return new SelfDamageHook();
                case "Curse_Rooted": return new RootedHook();
                
                default: 
                    Debug.LogWarning($"HookRegistry: Unrecognized hook ID {id}");
                    return null;
            }
        }
    }
}
