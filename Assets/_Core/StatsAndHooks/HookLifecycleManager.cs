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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ApplyContract(ContractModel model)
        {
            ClearAllHooks();

            if (model.BoonNodeIDs != null)
            {
                foreach (var id in model.BoonNodeIDs)
                {
                    var hook = HookRegistry.InstantiateHook(id);
                    if (hook != null) { hook.Enable(); _activeHooks.Add(hook); }
                }
            }

            if (model.CurseNodeIDs != null)
            {
                foreach (var id in model.CurseNodeIDs)
                {
                    var hook = HookRegistry.InstantiateHook(id);
                    if (hook != null) { hook.Enable(); _activeHooks.Add(hook); }
                }
            }

            // Apply base stat modifiers from AI
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Simple flat modifiers based on AI's multiplier output
                Faust.Simulation.PlayerController.Instance.BaseDamage = 10f * model.DamageModifier;
                Faust.Simulation.PlayerController.Instance.BaseProjectileSpeed = 20f * model.SpeedModifier;
            }
            
            Debug.Log($"Contract Applied: {model.ItemName}");
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
