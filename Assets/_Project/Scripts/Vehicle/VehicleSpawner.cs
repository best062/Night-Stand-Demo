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

        [Header("Timing & Queue")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField, Min(0.2f)] private float spawnDelayBetweenVehicles = 2.0f;
        [SerializeField, Min(1)] private int queueBufferSize = 3;
        [Tooltip("หากเปิดไว้ ลูกบ้าน Normal_Valid แต่ละคนจะผ่านเข้าหมู่บ้านได้แค่ 1 ครั้ง (คันเดียว)")]
        [SerializeField] private bool allowEachValidResidentOnce = true;

        [Header("Dependencies")]
        [SerializeField] private TimeWaveManager timeWaveManager;

        public VehicleController CurrentVehicle { get; private set; }
        public int QueuedCount => vehicleQueue.Count;

        // [Data Structure: Queue<VehicleDataSO> - FIFO Vehicle Spawn Queue]
        // Stores upcoming vehicles in arrival order before spawning at the checkpoint.
        // Time Complexity: Enqueue O(1), Dequeue O(1)
        private readonly Queue<VehicleDataSO> vehicleQueue = new Queue<VehicleDataSO>();

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
            EnqueueUpcomingVehicles(queueBufferSize);

            if (spawnOnStart)
            {
                SpawnNextVehicle();
            }
        }

        // [Operation: FIFO Enqueue O(1) per vehicle]
        public void EnqueueUpcomingVehicles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                VehicleDataSO candidate = PickRandomVehicleData();
                if (candidate != null)
                {
                    vehicleQueue.Enqueue(candidate);
                }
            }
        }

        // [Operation: FIFO Dequeue O(1) when spawning the next vehicle to the gate]
        public void SpawnNextVehicle()
        {
            if (!CanSpawn())
            {
                return;
            }

            if (vehicleQueue.Count == 0)
            {
                EnqueueUpcomingVehicles(queueBufferSize);
            }

            if (vehicleQueue.Count == 0)
            {
                Debug.LogWarning("[VehicleSpawner] Vehicle Pool is empty! Add VehicleDataSO assets in Inspector.");
                return;
            }

            VehicleDataSO selectedData = vehicleQueue.Dequeue();
            EnqueueUpcomingVehicles(1);

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            VehicleController spawned = Instantiate(vehiclePrefab, pos, rot);
            CurrentVehicle = spawned;
            CurrentVehicle.OnVehicleExited += HandleVehicleExited;

            GameWave currentWave = timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
            CurrentVehicle.Initialize(selectedData, gateStopPoint, currentWave);
        }

        public void AdminSpawnVehicleOfStatus(NPCStatus targetStatus)
        {
            if (vehiclePrefab == null || vehiclePool == null || vehiclePool.Count == 0)
            {
                return;
            }

            List<VehicleDataSO> matches = new List<VehicleDataSO>();
            foreach (VehicleDataSO data in vehiclePool)
            {
                if (data != null && data.NpcData != null && data.EffectiveStatus == targetStatus)
                {
                    matches.Add(data);
                }
            }

            if (matches.Count == 0)
            {
                Debug.LogWarning($"[VehicleSpawner] No vehicles found with status {targetStatus}.");
                return;
            }

            if (CurrentVehicle != null)
            {
                CurrentVehicle.OnVehicleExited -= HandleVehicleExited;
                Destroy(CurrentVehicle.gameObject);
                CurrentVehicle = null;
            }

            if (pendingSpawnCoroutine != null)
            {
                StopCoroutine(pendingSpawnCoroutine);
                pendingSpawnCoroutine = null;
            }

            VehicleDataSO chosen = matches[Random.Range(0, matches.Count)];
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            VehicleController spawned = Instantiate(vehiclePrefab, pos, rot);
            CurrentVehicle = spawned;
            CurrentVehicle.OnVehicleExited += HandleVehicleExited;

            GameWave currentWave = timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
            CurrentVehicle.Initialize(chosen, gateStopPoint, currentWave);
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

                switch (data.EffectiveStatus)
                {
                    case NPCStatus.Normal_Valid:
                        if (allowEachValidResidentOnce &&
                            ResidentDatabase.Instance != null &&
                            ResidentDatabase.Instance.IsDoppelganger(data.FrontLicensePlate))
                        {
                            // ลูกบ้านคนนี้เข้าไปแล้ว ห้ามสุ่มตัวจริงซ้ำอีก (เหลือแค่ตัวปลอม/Anomaly)
                            break;
                        }
                        validNormals.Add(data);
                        break;
                    case NPCStatus.Normal_Invalid:
                        invalidNormals.Add(data);
                        break;
                    case NPCStatus.Anomaly:
                        AnomalyDataSO anomaly = data.EffectiveAnomalyData;
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
