using System;
using UnityEngine;

namespace Faust.Rails
{
    // Agent B: Stub created to unblock hook implementation
    public class PlayerContext : MonoBehaviour
    {
        public static PlayerContext Instance { get; private set; }

        public float Health = 100f;
        public float MaxHealth = 100f;
        public float MoveSpeed = 5f;
        
        [HideInInspector]
        public bool IsRooted = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void TakeDamage(float amount)
        {
            Health = Mathf.Max(0, Health - amount);
            CombatEventBus.OnPlayerDamaged?.Invoke(amount);
        }

        public void Heal(float amount)
        {
            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        public static void Reset()
        {
            if (Instance != null)
            {
                Instance.Health = Instance.MaxHealth;
                Instance.IsRooted = false;
                Instance.transform.position = Vector3.zero;
            }
        }
    }
}
