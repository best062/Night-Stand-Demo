using System;
using System.Collections.Generic;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Items
{
    [Serializable]
    public class DeskSpawnEntry
    {
        public string label = "Desk Item";
        public Transform spawnPoint;
        public GameObject itemPrefab;
        public DocumentDataSO overrideDocumentData;
        public bool spawnOnStart = true;

        [HideInInspector] public GameObject spawnedInstance;
    }

    /// <summary>
    /// ระบบ Item Spawn (2p) บนโต๊ะป้อมยาม สำหรับวางเอกสารรายชื่อลูกบ้านและบัตรผ่าน (หรืออุปกรณ์เสริม)
    /// </summary>
    public class DeskItemSpawner : MonoBehaviour
    {
        [Header("Desk Spawn Points")]
        [SerializeField] private List<DeskSpawnEntry> spawnEntries = new List<DeskSpawnEntry>();

        private void Start()
        {
            SpawnAllInitialItems();
        }

        public void SpawnAllInitialItems()
        {
            for (int i = 0; i < spawnEntries.Count; i++)
            {
                if (spawnEntries[i].spawnOnStart)
                {
                    SpawnEntry(i);
                }
            }
        }

        public GameObject SpawnEntry(int index)
        {
            if (index < 0 || index >= spawnEntries.Count)
            {
                return null;
            }

            DeskSpawnEntry entry = spawnEntries[index];
            if (entry.itemPrefab == null)
            {
                Debug.LogWarning($"[DeskItemSpawner] Missing itemPrefab at index {index} ({entry.label}).");
                return null;
            }

            if (entry.spawnedInstance != null)
            {
                Destroy(entry.spawnedInstance);
            }

            Vector3 pos = entry.spawnPoint != null ? entry.spawnPoint.position : transform.position;
            Quaternion rot = entry.spawnPoint != null ? entry.spawnPoint.rotation : transform.rotation;
            Transform parent = entry.spawnPoint != null ? entry.spawnPoint : transform;

            GameObject instance = Instantiate(entry.itemPrefab, pos, rot, parent);
            entry.spawnedInstance = instance;

            if (entry.overrideDocumentData != null)
            {
                DocumentInteractable docInteractable = instance.GetComponentInChildren<DocumentInteractable>();
                if (docInteractable != null)
                {
                    docInteractable.SetDocumentData(entry.overrideDocumentData);
                }
            }

            return instance;
        }
    }
}
