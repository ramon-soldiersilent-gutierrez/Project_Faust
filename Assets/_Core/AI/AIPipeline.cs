using System;
using UnityEngine;
using Faust.Rails;

namespace Faust.AI
{
    // Mock Agent D implementation to ensure pipeline completes even before LLM is wired
    public class AIPipeline : MonoBehaviour
    {
        public static AIPipeline Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RequestContract(string wish, float greed, Action<ContractModel> onComplete)
        {
            Debug.Log($"AI requested: Wish='{wish}', Greed={greed}");
            
            // TODO: Wire up actual Gemini API call here with 5s timeout.
            // On timeout or parse error, fall back to FallbackCompiler.
            
            onComplete?.Invoke(FallbackCompiler.GenerateSafeContract(wish, greed));
        }
    }

    public static class FallbackCompiler
    {
        public static ContractModel GenerateSafeContract(string wish, float greed)
        {
            // If the LLM burns down, we still demo perfectly.
            return new ContractModel
            {
                ItemName = "The Deterministic Blade",
                FlavorText = "The API failed, but the demo must go on.",
                GrantedSkillID = "Kinetic_Projectile",
                BoonNodeIDs = new[] { "Boon_Multicast" },
                CurseNodeIDs = new[] { "Curse_GlassCannon" },
                DamageModifier = 1.0f + (greed / 100f),
                SpeedModifier = 1.0f,
                SizeModifier = 1.0f
            };
        }
    }
}
