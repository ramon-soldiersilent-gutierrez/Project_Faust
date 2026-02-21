using System;
using System.Threading.Tasks;
using UnityEngine;
using Faust.Rails;

namespace Faust.AI
{
    public class AIPipeline : MonoBehaviour, ILogSink
    {
        public static AIPipeline Instance { get; private set; }

        [Header("Gemini Configuration")]
        [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";
        [SerializeField] private int timeoutSeconds = 5;

        private GeminiClient _client;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _client = new GeminiClient(apiKey);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async void RequestContract(string wish, float greed, Action<ContractModel> onComplete)
        {
            Log($"AI requested: Wish='{wish}', Greed={greed}");

            string systemPrompt = BuildSystemPrompt(wish, greed);
            string userPrompt = $"My wish is '{wish}'. Make my contract!";

            try
            {
                string jsonResponse = await _client.SendPromptAsync(systemPrompt, userPrompt, timeoutSeconds);
                Log($"Raw JSON received:\n{jsonResponse}");

                ContractModel model = ContractParser.ParseAndValidate(jsonResponse, this);
                onComplete?.Invoke(model);
            }
            catch (Exception e)
            {
                LogError($"API or Parse Failed: {e.Message}. Falling back.");
                onComplete?.Invoke(FallbackCompiler.GenerateSafeContract(wish, greed));
            }
        }

        private string BuildSystemPrompt(string wish, float greed)
        {
            return $@"You are the Faustian Contract Generator.
The player's wish is '{wish}'. Their greed tier is {greed} / 100.
You must return only raw JSON matching this literal schema exactly without any markdown tags:
{{
  ""itemName"": ""string"",
  ""flavorText"": ""string"",
  ""skillPref"": ""Kinetic_Projectile"" or ""Kinetic_Sweep"",
  ""tags"": [""Fire"",""Projectile"",""Melee"",""Speed"",""Self""],
  ""boons"": [{{ ""id"": ""Boon_DamageSpike"" or ""Boon_MachineGun"" or ""Boon_Multicast"" or ""Boon_Vampiric"", ""magnitude"": 1.0 }}],
  ""curses"": [{{ ""id"": ""Curse_TeleportOnHit"" or ""Curse_GlassCannon"" or ""Curse_SelfDamage"" or ""Curse_Rooted"", ""magnitude"": 1.0 }}],
  ""damageModifier"": 1.0,
  ""speedModifier"": 1.0,
  ""sizeModifier"": 1.0
}}";
        }

        public void Log(string message) => Debug.Log($"[AI] {message}");
        public void LogWarning(string message) => Debug.LogWarning($"[AI] {message}");
        public void LogError(string message) => Debug.LogError($"[AI] {message}");
    }

    public static class FallbackCompiler
    {
        public static ContractModel GenerateSafeContract(string wish, float greed)
        {
            var model = new ContractModel
            {
                ItemName = "The Deterministic Blade",
                FlavorText = $"The API failed, but the demo must go on.",
                GrantedSkillID = "Kinetic_Projectile",
                DamageModifier = 1.0f + (greed / 100f),
                SpeedModifier = 1.0f,
                SizeModifier = 1.0f
            };

            // Deterministic Greed Tiers from invariants.md:
            // 0–30: 1 boon (mild) + 1 curse (mild)
            // 31–70: 1 boon (stronger) + 1 curse (spicy)
            // 71–100: 1 boon (max) + 1 signature curse (Glass Cannon / One-Punch class effects)

            if (greed <= 30f)
            {
                model.BoonNodeIDs = new[] { "Boon_MachineGun" };
                model.CurseNodeIDs = new[] { "Curse_Rooted" };
            }
            else if (greed <= 70f)
            {
                model.BoonNodeIDs = new[] { "Boon_Multicast" };
                model.CurseNodeIDs = new[] { "Curse_TeleportOnHit" };
                model.DamageModifier += 0.5f;
            }
            else
            {
                model.BoonNodeIDs = new[] { "Boon_DamageSpike" }; // "max boon"
                model.CurseNodeIDs = new[] { "Curse_GlassCannon" }; // "signature curse"
                model.DamageModifier += 1.5f;
                // Add extreme spice
                model.SizeModifier += 0.5f;
            }

            return model;
        }
    }
}
