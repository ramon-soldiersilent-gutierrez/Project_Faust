using UnityEngine;
using Faust.Rails;

namespace Faust.StatsAndHooks
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public int CurrentLevel { get; private set; } = 1;
        public float CurrentXP { get; private set; } = 0f;
        public float XpToNextLevel { get; private set; } = 100f;
        public int AvailableSkillPoints { get; private set; } = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            CombatEventBus.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            CombatEventBus.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(float xpValue)
        {
            CurrentXP += xpValue;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            if (CurrentXP >= XpToNextLevel)
            {
                // Calculate overflow
                CurrentXP -= XpToNextLevel;
                
                // Increment stats
                CurrentLevel++;
                AvailableSkillPoints += 2;
                XpToNextLevel = CurrentLevel * 100f; // Scale requirements

                // Broadcast
                CombatEventBus.OnLevelUp?.Invoke(CurrentLevel, AvailableSkillPoints);
                
                Debug.Log($"<color=cyan>LEVEL UP! Level {CurrentLevel} reached. {AvailableSkillPoints} Skill Points available.</color>");

                // Recursively check in case they earned enough XP for multiple levels at once
                CheckLevelUp();
            }
        }
        
        // Let UI or other systems spend points
        public bool SpendSkillPoints(int amount)
        {
            if (AvailableSkillPoints >= amount)
            {
                AvailableSkillPoints -= amount;
                return true;
            }
            return false;
        }

        public void RefundSkillPoints(int amount)
        {
            AvailableSkillPoints += amount;
        }
        
        // For Demo Reset API
        public void ResetLeveling()
        {
            CurrentLevel = 1;
            CurrentXP = 0f;
            XpToNextLevel = 100f;
            AvailableSkillPoints = 0;
        }
    }
}
