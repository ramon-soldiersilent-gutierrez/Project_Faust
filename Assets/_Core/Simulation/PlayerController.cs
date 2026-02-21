using System.Collections;
using UnityEngine;
using Faust.Rails;

namespace Faust.Simulation
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        private Renderer _renderer;

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

            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _renderer.material.color = Color.green;
            }
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
            // Do not execute attacks if a menu is open
            if (Time.timeScale == 0f) return;
            
            // Note: In an actual implementation, these would read equipped AbilityContexts from Agent C (Inventory) or Agent B (Stats)
            // For Phase 8 Shell Polish, we invoke stubbed casts mapped to 6 action bar slots.
            
            if (Input.GetMouseButtonDown(0))      ExecuteSlot(0); // Mouse0
            else if (Input.GetMouseButtonDown(1)) ExecuteSlot(1); // Mouse1
        }

        private void ExecuteSlot(int slotIndex)
        {
            // For Phase 9, Mouse0 (0) = Kinetic_Projectile, Mouse1 (1) = Kinetic_Sweep
            // The rest are stubbed Projectiles for now
            
            SkillShape shape = SkillShape.Projectile;
            
            if (slotIndex == 1) // Mouse1
            {
                shape = SkillShape.ForwardSweep;
            }

            var ctx = new AbilityContext
            {
                ExecutionShape = shape,
                FinalDamage = BaseDamage,
                FinalProjectileSpeed = BaseProjectileSpeed,
                FinalProjectileCount = BaseProjectileCount,
                FinalAreaRadius = 3f, // Larger for sweep testing
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
                StartCoroutine(FlashDamageRoutine());
            }
            if (CurrentHealth <= 0)
            {
                if (Faust.UI.UIManager.Instance != null)
                {
                    Faust.UI.UIManager.Instance.ShowGameOver();
                }
            }
        }

        private IEnumerator FlashDamageRoutine()
        {
            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                _renderer.material.color = Color.green;
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
