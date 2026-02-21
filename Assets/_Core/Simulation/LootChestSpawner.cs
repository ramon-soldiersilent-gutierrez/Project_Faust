using System.Collections.Generic;
using UnityEngine;
using Faust.Rails;
using Faust.Simulation;

namespace Faust.Simulation
{
    public class LootChestSpawner : MonoBehaviour
    {
        public static LootChestSpawner Instance { get; private set; }

        [Header("Settings")]
        public GameObject ChestPrefab; // User assigns chest_close.prefab
        public float SpawnRadius = 1f; // Dispersion
        public Transform PlayerTransform;
        public float PickupRadius = 1.5f;

        private List<ChestBody> _activeChests = new List<ChestBody>();
        private Queue<Transform> _chestPool = new Queue<Transform>();

        private struct ChestBody
        {
            public Vector3 Position;
            public Transform VisualTransform;
        }

        private void Awake()
        {
            Instance = this;

            var root = new GameObject("LootChests_Pool").transform;
            for (int i = 0; i < 20; i++)
            {
                GameObject go;
                if (ChestPrefab != null)
                {
                    go = Instantiate(ChestPrefab);
                }
                else
                {
                    // Fallback Primitive if empty
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var renderer = go.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material.color = Color.yellow; // Chest color
                }
                
                go.name = $"Chest_{i}";
                go.transform.SetParent(root);
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Scaled appropriately
                
                go.transform.position = new Vector3(0, 0.25f, 0);
                if (go.GetComponent<Collider>() != null) Destroy(go.GetComponent<Collider>());
                go.SetActive(false);
                _chestPool.Enqueue(go.transform);
            }
        }

        private void Update()
        {
            if (PlayerTransform == null) return;
            
            // Distance-based pickup check
            Vector3 playerPos = PlayerTransform.position;
            for (int i = _activeChests.Count - 1; i >= 0; i--)
            {
                var chest = _activeChests[i];
                if (Vector3.Distance(chest.Position, playerPos) <= PickupRadius)
                {
                    OpenChest();
                    chest.VisualTransform.gameObject.SetActive(false);
                    _chestPool.Enqueue(chest.VisualTransform);
                    _activeChests.RemoveAt(i);
                }
            }
        }

        public void SpawnChestAt(Vector3 basePos)
        {
            if (_chestPool.Count == 0) return;
            
            float angle = Random.Range(0f, Mathf.PI * 2);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * SpawnRadius;
            Vector3 spawnPos = basePos + offset;
            spawnPos.y = 0.25f; 
            
            Transform cTrans = _chestPool.Dequeue();
            cTrans.position = spawnPos;
            cTrans.gameObject.SetActive(true);

            _activeChests.Add(new ChestBody
            {
                Position = spawnPos,
                VisualTransform = cTrans
            });
        }
            Vector3 playerPos = PlayerTransform.position;
            for (int i = _activeChests.Count - 1; i >= 0; i--)
            {
                var chest = _activeChests[i];
                if (Vector3.Distance(chest.Position, playerPos) <= PickupRadius)
                {
                    OpenChest();
                    chest.VisualTransform.gameObject.SetActive(false);
                    _chestPool.Enqueue(chest.VisualTransform);
                    _activeChests.RemoveAt(i);
                }
            }
        }

        private void SpawnChest()
        {
            if (_chestPool.Count == 0) return;
            
            float angle = Random.Range(0f, Mathf.PI * 2);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * SpawnRadius;
            Vector3 spawnPos = PlayerTransform.position + offset;
            spawnPos.y = 0.25f; 
            
            Transform cTrans = _chestPool.Dequeue();
            cTrans.position = spawnPos;
            cTrans.gameObject.SetActive(true);

            _activeChests.Add(new ChestBody
            {
                Position = spawnPos,
                VisualTransform = cTrans
            });
        }

        private void OpenChest()
        {
            string[] slots = new string[] { "Weapon", "Armor", "Accessory" };
            string chosenSlot = slots[Random.Range(0, slots.Length)];

            var loot = new ContractModel
            {
                ItemName = $"Dropped {chosenSlot}",
                FlavorText = "Looted from the abyss.",
                EquipSlot = chosenSlot,
                SpriteKeyword = $"{chosenSlot}_Generic",
                DamageModifier = Random.Range(1.05f, 1.25f),
                SpeedModifier = Random.Range(1.0f, 1.15f)
            };

            Debug.Log($"Looted: {loot.ItemName}! Adding to Stash.");
            if (Faust.UI.ContractUI.Instance != null)
            {
                Faust.UI.ContractUI.Instance.AddStashItem(loot);
            }
        }
    }
}
