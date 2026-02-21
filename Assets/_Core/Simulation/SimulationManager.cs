using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;

namespace Faust.Simulation
{
    // FakeECS implementation: Handles the core simulation loop without per-entity Updates
    public class SimulationManager : MonoBehaviour, ISimulationAPI
    {
        public static SimulationManager Instance { get; private set; }

        private readonly List<ProjectileBody> _activeProjectiles = new List<ProjectileBody>(200);
        private readonly List<EnemyBody> _activeEnemies = new List<EnemyBody>(100);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            // Tick all active projectiles
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _activeProjectiles[i];
                // TODO: Update position, distance-checked collision, despawn if necessary
                
                // _activeProjectiles[i] = proj;
            }

            // Tick all active enemies
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                // TODO: Update position, check distances to player
                
                // _activeEnemies[i] = enemy;
            }
        }

        // --- ISimulationAPI Implementation ---
        public void ExecuteSkill(in AbilityContext context)
        {
            // Agent A uses context.ExecutionShape to determine which generic spawner to call here.
            switch (context.ExecutionShape)
            {
                case SkillShape.Projectile:
                    // Spawn projectile using context.FinalProjectileSpeed, etc.
                    break;
                case SkillShape.ForwardSweep:
                    // Perform melee BoxCast or physics overlap
                    break;
                case SkillShape.TargetedAoE:
                    // Perform area blast
                    break;
            }
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
