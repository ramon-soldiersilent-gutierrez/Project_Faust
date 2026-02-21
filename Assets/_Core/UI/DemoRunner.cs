using System;
using UnityEngine;
using Faust.Rails;

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
            // HookLifecycleManager.Instance.ClearAllHooks();

            // 3. Purge all Simulation entities
            // SimulationManager.Instance.ClearAll();

            // 4. Restore Player Health/Pos
            // PlayerContext.Reset();

            // 5. Clear AI Console
            // AIConsole.Instance.Clear();
        }
    }

    public class ContractUI : MonoBehaviour
    {
        // TODO: Wire up text inputs for Wish, Slider for Greed (0-100)
        
        public void OnForgeButtonClicked()
        {
            // Disable button to prevent spam
            // string wish = view.GetWishText();
            // float greed = view.GetGreedSlider();

            // AgentD.Pipeline.RequestContract(wish, greed, OnContractReceived);
        }

        private void OnContractReceived(ContractModel model)
        {
            // Re-enable button
            // IContractRuntime.ApplyContract(model);
        }
    }
}
