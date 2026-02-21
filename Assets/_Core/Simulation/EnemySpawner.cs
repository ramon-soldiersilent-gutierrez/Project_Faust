using UnityEngine;
using Faust.Simulation;

namespace Faust.Simulation
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Settings")]
        public float SpawnInterval = 2.0f;
        public float SpawnRadius = 25f; // Ensures they spawn off-screen
        public Transform PlayerTransform;

        [Header("Enemy Stats")]
        public float BaseEnemyHealth = 50f;
        public float BaseEnemySpeed = 3.5f;

        private float _spawnTimer;

        private void Update()
        {
            if (SimulationManager.Instance == null || PlayerTransform == null) return;
            
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= SpawnInterval)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            // Pick a random angle
            float angle = Random.Range(0f, Mathf.PI * 2);
            
            // Calculate a point on that circle around the player
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * SpawnRadius;
            Vector3 spawnPos = PlayerTransform.position + offset;
            spawnPos.y = 0.5f; // Force enemies to stay on the floor plane
            
            // 20% chance for Ranged (enum value 1), 80% for Melee (enum value 0)
            int enemyType = Random.value < 0.2f ? 1 : 0;
            
            SimulationManager.Instance.SpawnEnemy(spawnPos, BaseEnemyHealth, BaseEnemySpeed, enemyType);
        }
    }
}
