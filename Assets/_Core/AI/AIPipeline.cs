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
            Log($"AI Contract requested: Wish='{wish}', Greed={greed}");

            string systemPrompt = BuildContractSystemPrompt(wish, greed);
            string userPrompt = $"My wish is '{wish}'. Make my contract!";

            try
            {
                string jsonResponse = await _client.SendPromptAsync(systemPrompt, userPrompt, timeoutSeconds);
                Log($"Contract JSON received:\n{jsonResponse}");

                ContractModel model = ContractParser.ParseAndValidate(jsonResponse, this);
                onComplete?.Invoke(model);
            }
            catch (Exception e)
            {
                LogError($"Contract API or Parse Failed: {e.Message}. Falling back.");
                onComplete?.Invoke(FallbackCompiler.GenerateSafeContract(wish, greed));
            }
        }

        public async void RequestSkillTreeChunk(int playerLevel, string currentSkills, string theme, Action<SkillTreeChunk> onComplete)
        {
            Log($"AI Skill Tree requested: Theme='{theme}'");

            string systemPrompt = BuildSkillTreeSystemPrompt(playerLevel, currentSkills, theme);
            string userPrompt = $"Generate a new skill tree chunk with the theme: '{theme}'";

            try
            {
                string jsonResponse = await _client.SendPromptAsync(systemPrompt, userPrompt, timeoutSeconds);
                Log($"Skill Tree JSON received:\n{jsonResponse}");

                SkillTreeChunk chunk = SkillTreeParser.ParseAndValidate(jsonResponse, this);
                onComplete?.Invoke(chunk);
            }
            catch (Exception e)
            {
                LogError($"Skill Tree API or Parse Failed: {e.Message}. Falling back.");
                onComplete?.Invoke(FallbackCompiler.GenerateSafeSkillTreeChunk(theme));
            }
        }

        private string BuildContractSystemPrompt(string wish, float greed)
        {
            return $@"You are the Faustian Contract Generator.
The player's wish is '{wish}'. Their greed tier is {greed} / 100.
You must return only raw JSON matching this literal schema exactly without any markdown tags:
{{
  ""itemName"": ""string"",
  ""flavorText"": ""string"",
  ""equipSlot"": ""Weapon""|""Armor""|""Accessory"",
  ""spriteKeyword"": ""[Type]_[Theme]"",
  ""skillPref"": ""Kinetic_Projectile"" or ""Kinetic_Sweep"",
  ""tags"": [""Fire"",""Projectile"",""Melee"",""Speed"",""Self""],
  ""boons"": [{{ ""id"": ""Boon_DamageSpike"" or ""Boon_MachineGun"" or ""Boon_Multicast"" or ""Boon_Vampiric"", ""magnitude"": 1.0 }}],
  ""curses"": [{{ ""id"": ""Curse_TeleportOnHit"" or ""Curse_GlassCannon"" or ""Curse_SelfDamage"" or ""Curse_Rooted"", ""magnitude"": 1.0 }}],
  ""damageModifier"": 1.0,
  ""speedModifier"": 1.0,
  ""sizeModifier"": 1.0
}}";
        }

        private string BuildSkillTreeSystemPrompt(int playerLevel, string currentSkills, string theme)
        {
            return $@"You are the Faustian Forge. Generate a new 2D web of Skill Tree Nodes.
Mortal's Current Level: {playerLevel}
Mortal's Equipped Skills: {currentSkills}
Mortal's Theme Preference: ""{theme}""

Rules:
1. Generate 15-30 nodes. Node 0 is at (0,0).
2. Every node MUST have at least 1 `connectedNodeIDs`.
3. 20% MUST be `isKeystone`: true, and grant 1-2 boons and 1-2 curses.
Respond ONLY with valid JSON matching this schema:
{{
  ""chunkName"": ""string"",
  ""theme"": ""string"",
  ""nodes"": [
    {{
      ""nodeID"": ""string"",
      ""displayName"": ""string"",
      ""flavorText"": ""string"",
      ""connectedNodeIDs"": [""string""],
      ""gridX"": 0,
      ""gridY"": 0,
      ""isKeystone"": false,
      ""grantedBoons"": [{{ ""id"": ""string"", ""magnitude"": 1.0 }}],
      ""grantedCurses"": [{{ ""id"": ""string"", ""magnitude"": 1.0 }}],
      ""damageDelta"": 0.05,
      ""speedDelta"": 0.05,
      ""sizeDelta"": 0.0
    }}
  ]
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
                EquipSlot = "Weapon",
                SpriteKeyword = "Sword_Fallback",
                GrantedSkillID = "Kinetic_Projectile",
                DamageModifier = 1.0f + (greed / 100f),
                SpeedModifier = 1.0f,
                SizeModifier = 1.0f
            };

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
                model.SizeModifier += 0.5f;
            }

            return model;
        }

        public static SkillTreeChunk GenerateSafeSkillTreeChunk(string theme)
        {
            // Deterministic offline fallback chunk containing a very small graph
            return new SkillTreeChunk
            {
                ChunkName = "The Offline Path",
                Theme = "Fallback Isolation",
                Nodes = new[]
                {
                    new SkillTreeNode
                    {
                        NodeID = "stubs_root",
                        DisplayName = "Start Here",
                        FlavorText = "Network failure detected.",
                        ConnectedNodeIDs = new[] { "stubs_end" },
                        GridX = 0,
                        GridY = 0,
                        IsKeystone = false,
                        DamageDelta = 0.05f
                    },
                    new SkillTreeNode
                    {
                        NodeID = "stubs_end",
                        DisplayName = "Fallback Keystone",
                        FlavorText = "The deterministic end.",
                        ConnectedNodeIDs = new string[0],
                        GridX = 1,
                        GridY = 0,
                        IsKeystone = true,
                        GrantedBoonIDs = new[] { "Boon_MachineGun" },
                        GrantedCurseIDs = new[] { "Curse_SelfDamage" }
                    }
                }
            };
        }
    }
}
