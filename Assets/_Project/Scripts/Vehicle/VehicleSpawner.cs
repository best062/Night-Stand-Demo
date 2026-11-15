using System.Collections;
using System.Collections.Generic;
using NightStand.Core;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Vehicle
{
    /// <summary>
    /// ระบบสุ่มและสร้างคิวรถยนต์ (Object Spawn) ตามน้ำหนักโอกาสและ Wave ปัจจุบัน
    /// </summary>
    public class VehicleSpawner : MonoBehaviour
    {
        [Header("Prefab & Spawn Points")]
        [SerializeField] private VehicleController vehiclePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform gateStopPoint;

        [Header("Vehicle Data Pool")]
        [SerializeField] private List<VehicleDataSO> vehiclePool = new List<VehicleDataSO>();

        [Header("Spawn Probabilities (Relative Weights)")]
        [Tooltip("โอกาสออก Normal_Valid (ควรสูงที่สุด)")]
        [SerializeField, Min(0f)] private float normalValidWeight = 60f;

        [Tooltip("โอกาสออก Anomaly (รองลงมาตาม Wave)")]
        [SerializeField, Min(0f)] private float anomalyWeight = 30f;

        [Tooltip("โอกาสออก Normal_Invalid (น้อยที่สุด)")]
        [SerializeField, Min(0f)] private float normalInvalidWeight = 10f;

        [Header("Timing")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField, Min(0.2f)] private float spawnDelayBetweenVehicles = 2.0f;

        [Header("Dependencies")]
        [SerializeField] private TimeWaveManager timeWaveManager;

        public VehicleController CurrentVehicle { get; private set; }

        private Coroutine pendingSpawnCoroutine;

        private void Awake()
        {
            if (timeWaveManager == null)
            {
                timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
            }
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnNextVehicle();
            }
        }

        public void SpawnNextVehicle()
        {
            if (!CanSpawn())
            {
                return;
            }

            VehicleDataSO selectedData = PickRandomVehicleData();
            if (selectedData == null)
            {
                Debug.LogWarning("[VehicleSpawner] Vehicle Pool is empty! Add VehicleDataSO assets in Inspector.");
                return;
            }

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            VehicleController spawned = Instantiate(vehiclePrefab, pos, rot);
            CurrentVehicle = spawned;
            CurrentVehicle.OnVehicleExited += HandleVehicleExited;

            GameWave currentWave = timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
            CurrentVehicle.Initialize(selectedData, gateStopPoint, currentWave);
        }

        private void HandleVehicleExited(VehicleController exitedVehicle)
        {
            if (exitedVehicle != null)
            {
                exitedVehicle.OnVehicleExited -= HandleVehicleExited;
            }

            if (CurrentVehicle == exitedVehicle)
            {
                CurrentVehicle = null;
            }

            if (pendingSpawnCoroutine != null)
            {
                StopCoroutine(pendingSpawnCoroutine);
            }

            if (CanSpawn())
            {
                pendingSpawnCoroutine = StartCoroutine(SpawnAfterDelay());
            }
        }

        private IEnumerator SpawnAfterDelay()
        {
            yield return new WaitForSeconds(spawnDelayBetweenVehicles);
            pendingSpawnCoroutine = null;
            SpawnNextVehicle();
        }

        private bool CanSpawn()
        {
            if (vehiclePrefab == null || CurrentVehicle != null)
            {
                return false;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return false;
            }

            if (timeWaveManager != null && timeWaveManager.HasShiftEnded)
            {
                return false;
            }

            return true;
        }

        private VehicleDataSO PickRandomVehicleData()
        {
            if (vehiclePool == null || vehiclePool.Count == 0)
            {
                return null;
            }

            GameWave currentWave = timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;

            List<VehicleDataSO> validNormals = new List<VehicleDataSO>();
            List<VehicleDataSO> invalidNormals = new List<VehicleDataSO>();
            List<VehicleDataSO> waveAnomalies = new List<VehicleDataSO>();

            foreach (VehicleDataSO data in vehiclePool)
            {
                if (data == null || data.NpcData == null)
                {
                    continue;
                }

                switch (data.NpcData.Status)
                {
                    case NPCStatus.Normal_Valid:
                        validNormals.Add(data);
                        break;
                    case NPCStatus.Normal_Invalid:
                        invalidNormals.Add(data);
                        break;
                    case NPCStatus.Anomaly:
                        AnomalyDataSO anomaly = data.NpcData.AnomalyData;
                        if (anomaly == null || (int)anomaly.AppearWave <= (int)currentWave)
                        {
                            waveAnomalies.Add(data);
                        }
                        break;
                }
            }

            float wValid = validNormals.Count > 0 ? normalValidWeight : 0f;
            float wAnomaly = waveAnomalies.Count > 0 ? anomalyWeight : 0f;
            float wInvalid = invalidNormals.Count > 0 ? normalInvalidWeight : 0f;
            float totalWeight = wValid + wAnomaly + wInvalid;

            if (totalWeight <= 0f)
            {
                return vehiclePool[Random.Range(0, vehiclePool.Count)];
            }

            float roll = Random.Range(0f, totalWeight);

            if (roll < wValid)
            {
                return validNormals[Random.Range(0, validNormals.Count)];
            }

            if (roll < wValid + wAnomaly)
            {
                return waveAnomalies[Random.Range(0, waveAnomalies.Count)];
            }

            return invalidNormals[Random.Range(0, invalidNormals.Count)];
        }
    }
}
