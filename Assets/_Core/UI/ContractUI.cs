using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Faust.Rails;
using System.Collections;

namespace Faust.UI
{
    public class ContractUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField wishInput;
        [SerializeField] private Slider greedSlider;
        [SerializeField] private Button forgeButton;

        private void Start()
        {
            if (forgeButton != null)
            {
                forgeButton.onClick.AddListener(OnForgeButtonClicked);
            }
        }

        public void OnForgeButtonClicked()
        {
            if (wishInput == null || greedSlider == null || forgeButton == null)
            {
                Debug.LogError("ContractUI: Missing UI references.");
                return;
            }

            // Disable button to prevent spam while waiting for async AI response
            forgeButton.interactable = false;

            string wish = wishInput.text;
            float greed = greedSlider.value; // expected 0-100 scale

            // Deterministic Greed Tiers
            // 0-30: Mild
            // 31-70: Medium
            // 71-100: Spicy
            int greedTier = 0;
            if (greed <= 30) greedTier = 1;
            else if (greed <= 70) greedTier = 2;
            else greedTier = 3;

            AIConsole.Instance?.Log($"Requesting Contract: Wish='{wish}', Greed={greed} (Tier {greedTier})");

            // TODO: Call Agent D's pipeline here once they merge.
            // Example: AgentD.ContractService.RequestContract(wish, greedTier, OnContractReceived, OnContractFailed);
            
            // For now in this worktree, we simulate an async response:
            StartCoroutine(SimulateAIResponse(wish, greedTier));
        }

        private IEnumerator SimulateAIResponse(string wish, int greedTier)
        {
            yield return new WaitForSeconds(1.5f);
            
            var stubModel = new ContractModel
            {
                ItemName = "Stub Contract",
                FlavorText = "A placeholder answer.",
                GrantedSkillID = "Kinetic_Projectile",
                BoonNodeIDs = new string[] { "Boon_DamageSpike" },
                CurseNodeIDs = new string[] { "Curse_GlassCannon" }
            };

            OnContractReceived(stubModel);
        }

        private void OnContractReceived(ContractModel model)
        {
            AIConsole.Instance?.Log($"Received Contract:\n{JsonUtility.ToJson(model, true)}");

            // Re-enable button
            if (forgeButton != null)
                forgeButton.interactable = true;

            // TODO: Apply the contract via the global runtime once integrated
            // Example: GlobalContractRuntime.Instance.ApplyContract(model); 
        }
        
        private void OnContractFailed(string errorMsg)
        {
             AIConsole.Instance?.LogError($"Contract Failed: {errorMsg}");
             if (forgeButton != null)
                forgeButton.interactable = true;
        }
    }
}
