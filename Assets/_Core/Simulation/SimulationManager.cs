using System;
using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.Simulation
{
    // FakeECS implementation: Handles the core simulation loop without per-entity Updates
    public class SimulationManager : MonoBehaviour, ISimulationAPI, IDemoAPI
    {
        public static SimulationManager Instance { get; private set; }

        private readonly List<ProjectileBody> _activeProjectiles = new List<ProjectileBody>(200);
        private readonly List<EnemyBody> _activeEnemies = new List<EnemyBody>(100);

        private readonly Queue<Transform> _projectilePool = new Queue<Transform>(200);
        private readonly Queue<Transform> _enemyPool = new Queue<Transform>(100);

        [Header("Setup")]
        public Transform PlayerTransform;
        public float HitRadius = 0.5f;   // Simple generic hit radius for detection

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePools();
        }

        private void InitializePools()
        {
            var projRoot = new GameObject("Projectiles_Pool").transform;
            for (int i = 0; i < 200; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Projectile_{i}";
                go.transform.SetParent(projRoot);
                go.transform.localScale = Vector3.one * 0.25f;
                // Remove collider to save performance, using distance checks
                Destroy(go.GetComponent<Collider>());
                go.SetActive(false);
                _projectilePool.Enqueue(go.transform);
            }

            var enemyRoot = new GameObject("Enemies_Pool").transform;
            for (int i = 0; i < 100; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Enemy_{i}";
                go.transform.SetParent(enemyRoot);
                go.transform.localScale = Vector3.one;
                Destroy(go.GetComponent<Collider>());
                go.SetActive(false);
                _enemyPool.Enqueue(go.transform);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 playerPos = PlayerTransform != null ? PlayerTransform.position : Vector3.zero;

            // Tick all active projectiles
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _activeProjectiles[i];
                proj.Position += proj.Velocity * dt;
                proj.DistanceTraveled += (proj.Velocity * dt).magnitude;
                
                proj.VisualTransform.position = proj.Position;

                bool despawned = false;

                // Hit Detection (Distance based)
                for (int j = _activeEnemies.Count - 1; j >= 0; j--)
                {
                    var enemy = _activeEnemies[j];
                    if (Vector3.Distance(proj.Position, enemy.Position) <= HitRadius)
                    {
                        // Hit registered
                        CombatEventBus.OnHit?.Invoke(new HitInfo
                        {
                            Target = enemy.VisualTransform,
                            Instigator = PlayerTransform,
                            Context = proj.Context
                        });

                        enemy.currentHealth -= proj.Context.FinalDamage;
                        
                        if (enemy.currentHealth <= 0)
                        {
                            enemy.VisualTransform.gameObject.SetActive(false);
                            _enemyPool.Enqueue(enemy.VisualTransform);
                            _activeEnemies.RemoveAt(j);
                        }
                        else
                        {
                            _activeEnemies[j] = enemy; // Apply health change
                        }

                        despawned = true;
                        break; // Projectile dies on first hit for now
                    }
                }

                if (despawned || proj.DistanceTraveled > 25f) // Despawn after distance
                {
                    proj.VisualTransform.gameObject.SetActive(false);
                    _projectilePool.Enqueue(proj.VisualTransform);
                    _activeProjectiles.RemoveAt(i);
                }
                else
                {
                    _activeProjectiles[i] = proj; // Apply struct changes
                }
            }

            // Tick all active enemies
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                
                // Move directly toward player
                Vector3 dirToPlayer = (playerPos - enemy.Position).normalized;
                enemy.Position += dirToPlayer * enemy.MoveSpeed * dt;
                enemy.VisualTransform.position = enemy.Position;
                
                _activeEnemies[i] = enemy;
            }
        }

        // --- ISimulationAPI Implementation ---
        public void ExecuteSkill(in AbilityContext context)
        {
            int pCount = Mathf.Max(1, context.FinalProjectileCount);

            switch (context.ExecutionShape)
            {
                case SkillShape.Projectile:
                    Vector3 spawnPos = PlayerTransform != null ? PlayerTransform.position + Vector3.up * 0.5f : Vector3.zero;
                    Vector3 forward = PlayerTransform != null ? PlayerTransform.forward : Vector3.forward;

                    for (int i = 0; i < pCount; i++)
                    {
                        if (_projectilePool.Count == 0)
                        {
                            Debug.LogWarning("Projectile pool empty!");
                            break;
                        }

                        // Spread logic
                        float angleOffset = (pCount > 1) ? Mathf.Lerp(-15f, 15f, i / (float)(pCount - 1)) : 0f;
                        Vector3 dir = Quaternion.Euler(0, angleOffset, 0) * forward;

                        Transform pTrans = _projectilePool.Dequeue();
                        pTrans.position = spawnPos;
                        pTrans.gameObject.SetActive(true);

                        var pBody = new ProjectileBody
                        {
                            Position = spawnPos,
                            Velocity = dir.normalized * context.FinalProjectileSpeed,
                            DistanceTraveled = 0f,
                            Context = context,
                            VisualTransform = pTrans
                        };

                        _activeProjectiles.Add(pBody);
                    }

                    CombatEventBus.OnCast?.Invoke(new CastInfo
                    {
                        Position = spawnPos,
                        Direction = forward,
                        Context = context
                    });
                    break;
                    
                case SkillShape.ForwardSweep:
                    // Simple positional hit detection
                    Vector3 sweepPos = PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
                    Vector3 sweepFwd = PlayerTransform != null ? PlayerTransform.forward : Vector3.forward;
                    float sweepRadius = context.FinalAreaRadius;

                    CombatEventBus.OnCast?.Invoke(new CastInfo
                    {
                        Position = sweepPos,
                        Direction = sweepFwd,
                        Context = context
                    });

                    for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = _activeEnemies[i];
                        Vector3 dirToEnemy = (enemy.Position - sweepPos).normalized;
                        if (Vector3.Distance(enemy.Position, sweepPos) <= sweepRadius && Vector3.Dot(sweepFwd, dirToEnemy) > 0.5f) // Roughly in front
                        {
                            CombatEventBus.OnHit?.Invoke(new HitInfo
                            {
                                Target = enemy.VisualTransform,
                                Instigator = PlayerTransform,
                                Context = context
                            });

                            enemy.currentHealth -= context.FinalDamage;
                            if (enemy.currentHealth <= 0)
                            {
                                enemy.VisualTransform.gameObject.SetActive(false);
                                _enemyPool.Enqueue(enemy.VisualTransform);
                                _activeEnemies.RemoveAt(i);
                            }
                            else
                            {
                                _activeEnemies[i] = enemy;
                            }
                        }
                    }
                    break;

                case SkillShape.TargetedAoE:
                    // Perform area blast
                    break;
            }
        }

        // --- IDemoAPI Implementation ---
        public void ResetAll()
        {
            foreach (var p in _activeProjectiles)
            {
                p.VisualTransform.gameObject.SetActive(false);
                _projectilePool.Enqueue(p.VisualTransform);
            }
            _activeProjectiles.Clear();

            foreach (var e in _activeEnemies)
            {
                e.VisualTransform.gameObject.SetActive(false);
                _enemyPool.Enqueue(e.VisualTransform);
            }
            _activeEnemies.Clear();

            // Reset TimeScale
            Time.timeScale = 1f;

            Debug.Log("Simulation Core Reset complete.");
        }

        // For testing/spawning enemies
        public void SpawnEnemy(Vector3 pos, float health, float speed)
        {
            if (_enemyPool.Count == 0) return;

            Transform eTrans = _enemyPool.Dequeue();
            eTrans.position = pos;
            eTrans.gameObject.SetActive(true);

            _activeEnemies.Add(new EnemyBody
            {
                Position = pos,
                MoveSpeed = speed,
                currentHealth = health,
                VisualTransform = eTrans
            });
        }
    }

    // Value types avoid allocation on the hot loop
    public struct ProjectileBody
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float DistanceTraveled;
        public AbilityContext Context;
        public Transform VisualTransform;
    }

    public struct EnemyBody
    {
        public Vector3 Position;
        public float MoveSpeed;
        public float currentHealth;
        public Transform VisualTransform;
    }
}
