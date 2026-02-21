using UnityEngine;
using Faust.Rails;

namespace Faust.Simulation
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Stats")]
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float CurrentHealth;

        [HideInInspector]
        public bool IsRooted = false;

        [Header("Progression")]
        public int StartingLevel = 1;
        public int CurrentLevel = 1;
        public float CurrentXP = 0f;
        public float XPPerLevel = 50f;
        public int SkillPoints = 0;

        [Header("Temp Default Skill")]
        public float BaseDamage = 10f;
        public float BaseProjectileSpeed = 20f;
        public int BaseProjectileCount = 1;

        private Camera _mainCamera;
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            _mainCamera = Camera.main;
            CurrentHealth = MaxHealth;
            CurrentLevel = StartingLevel;
            SkillPoints = Mathf.Max(0, CurrentLevel - 1);

            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }

        private void OnEnable()
        {
            CombatEventBus.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            CombatEventBus.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(float xp)
        {
            CurrentXP += xp;
            while (CurrentXP >= XPPerLevel)
            {
                CurrentXP -= XPPerLevel;
                CurrentLevel++;
                SkillPoints++;
                CombatEventBus.OnLevelUp?.Invoke(CurrentLevel, SkillPoints);
                Debug.Log($"Level Up! Now Level {CurrentLevel}");
            }
        }

        private void Update()
        {
            HandleMovement();
            HandleAiming();
            HandleInput();
        }

        private void HandleMovement()
        {
            if (IsRooted) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            transform.position += moveDir * MoveSpeed * Time.deltaTime;
        }

        private void HandleAiming()
        {
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 lookDir = (hitPoint - transform.position).normalized;
                lookDir.y = 0;
                
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
        }

        private void HandleInput()
        {
            // Note: In an actual implementation, these would read equipped AbilityContexts from Agent C (Inventory) or Agent B (Stats)
            // For Phase 8 Shell Polish, we invoke stubbed casts mapped to 6 action bar slots.
            
            if (Input.GetMouseButtonDown(0))      ExecuteSlot(0); // Mouse0
            else if (Input.GetMouseButtonDown(1)) ExecuteSlot(1); // Mouse1
            else if (Input.GetKeyDown(KeyCode.Alpha1)) ExecuteSlot(2); // Slot 1
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ExecuteSlot(3); // Slot 2
            else if (Input.GetKeyDown(KeyCode.Alpha3)) ExecuteSlot(4); // Slot 3
            else if (Input.GetKeyDown(KeyCode.Alpha4)) ExecuteSlot(5); // Slot 4
        }

        private void ExecuteSlot(int slotIndex)
        {
            // Fire default projectile skill for Agent A's requirement as a fallback generic test
            var ctx = new AbilityContext
            {
                ExecutionShape = SkillShape.Projectile,
                FinalDamage = BaseDamage,
                FinalProjectileSpeed = BaseProjectileSpeed,
                FinalProjectileCount = BaseProjectileCount,
                FinalAreaRadius = 1f,
                FinalCastTime = 0.1f
            };

            SimulationManager.Instance.ExecuteSkill(ctx);
        }

        public void TakeDamage(float amount, bool isCurseDamage = false)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            
            if (!isCurseDamage)
            {
                CombatEventBus.OnPlayerDamaged?.Invoke(amount);
            }
            if (CurrentHealth <= 0)
            {
                // Handle Death
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        // Called by DemoAPI (simulated) or externally
        public void ResetPlayer()
        {
            transform.position = Vector3.zero;
            CurrentHealth = MaxHealth;
            CurrentLevel = StartingLevel;
            SkillPoints = Mathf.Max(0, CurrentLevel - 1);
            CurrentXP = 0f;
            IsRooted = false;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null) return;

            // Simple isometric camera lock
            Vector3 targetCamPos = transform.position + new Vector3(0, 15f, -15f);
            _mainCamera.transform.position = targetCamPos;
            
            // Standard Isometric look angle
            _mainCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }
    }
}
