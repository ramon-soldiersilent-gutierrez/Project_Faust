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

        public void ActivateTreeNode(SkillTreeNode node)
        {
            if (!ActiveTreeNodes.Contains(node))
            {
                ActiveTreeNodes.Add(node);
                RefreshAll();
            }
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
                if (node.GrantedBoons != null)
                {
                    foreach (var id in node.GrantedBoons) { InjectHook(id); }
                }
                if (node.GrantedCurses != null)
                {
                    foreach (var id in node.GrantedCurses) { InjectHook(id); }
                }

                // Treat Tree Deltas as "Increased" additive modifiers
                treeDamageIncreased += node.DamageDelta;
                treeSpeedIncreased += node.SpeedDelta;
            }

            // 3. Compile Global Stats to PlayerController (Simulation reads from this)
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Damage = Base(10) * (1 + Sum(Increased)) * Product(More)
                Faust.Simulation.PlayerController.Instance.BaseDamage = 10f * (1.0f + treeDamageIncreased) * totalDamageMultiplier;
                Faust.Simulation.PlayerController.Instance.BaseProjectileSpeed = 20f * (1.0f + treeSpeedIncreased) * totalSpeedMultiplier;
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
