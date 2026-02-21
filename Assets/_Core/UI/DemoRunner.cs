using System;
using UnityEngine;
using Faust.Rails;
using Faust.StatsAndHooks;

namespace Faust.UI
{
    // Catcher for the F12 demo reset
    public class DemoRunner : MonoBehaviour, IDemoAPI
    {
        public static DemoRunner Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ResetAll();
            }
        }

        // --- IDemoAPI Implementation ---
        public void ResetAll()
        {
            Debug.Log("DEMO RESET TRIGGERED: Purging hooks, resetting timescale, restoring player.");
            
            // 1. Reset World Time
            Time.timeScale = 1f;

            // 2. Unsubscribe all active Boons/Curses
            if (HookLifecycleManager.Instance != null)
                HookLifecycleManager.Instance.ClearAllHooks();

            // 3. Purge all Simulation entities
            if (Faust.Simulation.SimulationManager.Instance != null)
                Faust.Simulation.SimulationManager.Instance.ResetAll();

            // 4. Restore Player Health/Pos
            if (Faust.Simulation.PlayerController.Instance != null)
                Faust.Simulation.PlayerController.Instance.ResetPlayer();

            // 5. Clear UI State
            if (ContractUI.Instance != null)
                ContractUI.Instance.ClearInventory();

            if (SkillTreeUI.Instance != null)
                SkillTreeUI.Instance.Clear();

            if (AIConsole.Instance != null)
                AIConsole.Instance.Clear();
        }
    }
}
