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
            
            Vector3 spawnPos = basePos;
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

            float rarityRoll = Random.value;
            string rarityName = "Common";
            float dmgBase = 1.0f;
            float spdBase = 1.0f;
            string boon = null;
            
            if (rarityRoll < 0.20f)
            {
                rarityName = "Rare";
                dmgBase = Random.Range(1.4f, 1.8f);
                spdBase = Random.Range(1.2f, 1.4f);
                string[] boons = { "Boon_DamageSpike", "Boon_Multicast", "Boon_Vampiric" };
                boon = boons[Random.Range(0, boons.Length)];
            }
            else if (rarityRoll < 0.60f)
            {
                rarityName = "Magic";
                dmgBase = Random.Range(1.15f, 1.35f);
                spdBase = Random.Range(1.05f, 1.15f);
            }
            else
            {
                rarityName = "Normal";
                dmgBase = Random.Range(1.0f, 1.10f);
                spdBase = Random.Range(0.95f, 1.05f);
            }

            var loot = new ContractModel
            {
                ItemName = $"{rarityName} {chosenSlot}",
                FlavorText = $"A {rarityName.ToLower()} find.",
                EquipSlot = chosenSlot,
                SpriteKeyword = $"{chosenSlot}_Generic",
                DamageModifier = dmgBase,
                SpeedModifier = spdBase,
                SizeModifier = 1.0f,
                BoonNodeIDs = boon != null ? new string[] { boon } : null
            };

            Debug.Log($"Looted: {loot.ItemName}! Adding to Stash.");
            if (Faust.UI.ContractUI.Instance != null)
            {
                Faust.UI.ContractUI.Instance.AddStashItem(loot);
            }
        }
    }
}
