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
        }

        private void OnEnable()
        {
            // Removed CombatEventBus subscription to prevent infinite loop recursion with hooks
        }

        private void OnDisable()
        {
            // Removed CombatEventBus subscription
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
            if (Input.GetMouseButtonDown(0))
            {
                // Fire default projectile skill for Agent A's requirement
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
        }

        public void TakeDamage(float amount, bool isCurseDamage = false)
        {
            CurrentHealth -= amount;
            
            if (!isCurseDamage)
            {
                CombatEventBus.OnPlayerDamaged?.Invoke(amount);
            }
            
            if (CurrentHealth <= 0)
            {
                // Debug.Log("Player Died!");
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        // Called by DemoAPI (simulated) or externally
        public void ResetPlayer()
        {
        public void ResetPlayer()
        {
            transform.position = Vector3.zero;
            CurrentHealth = MaxHealth;
            IsRooted = false;
        }
    }
}
