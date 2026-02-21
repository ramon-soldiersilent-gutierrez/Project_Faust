using System;
using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.StatsAndHooks
{
    public class HookLifecycleManager : MonoBehaviour
    {
        public static HookLifecycleManager Instance { get; private set; }

        // The active list of injected gameplay logic behavior hooks
        private List<IHookInstance> _activeHooks = new List<IHookInstance>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InjectHooks(List<SpecializationNodeSO> activeNodes)
        {
            // First disable and clear all old hooks (Reset behavior)
            ClearAllHooks();

            foreach (var node in activeNodes)
            {
                if (string.IsNullOrEmpty(node.InjectHookID)) continue;

                var newHook = HookRegistry.InstantiateHook(node.InjectHookID);
                if (newHook != null)
                {
                    newHook.Enable();
                    _activeHooks.Add(newHook);
                }
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
