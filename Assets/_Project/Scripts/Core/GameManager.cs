using System;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// Singleton หลักสำหรับเก็บสถานะการเล่น สถิติความผิดพลาด และตัดสินผลลัพธ์ของกะ
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private TimeWaveManager timeWaveManager;

        [Header("Lifecycle")]
        [SerializeField] private bool persistAcrossScenes = false;

        public event Action OnStatsUpdated;
        public event Action<GameEndResult> OnGameEnded;

        public int VehiclesAllowedCount { get; private set; }
        public int VehiclesDeniedCount { get; private set; }
        public int VehiclesEliminatedCount { get; private set; }
        public int MistakeCount { get; private set; }
        public int AnomaliesInfiltratedCount { get; private set; }
        public GameWave CurrentWave { get; private set; } = GameWave.Wave1;
        public GameEndResult EndResult { get; private set; } = GameEndResult.None;
        public bool IsGameOver => EndResult != GameEndResult.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (timeWaveManager == null)
            {
                timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
            }
        }

        private void OnEnable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged += HandleWaveChanged;
                timeWaveManager.OnShiftEnded += HandleShiftEnded;
            }
        }

        private void OnDisable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged -= HandleWaveChanged;
                timeWaveManager.OnShiftEnded -= HandleShiftEnded;
            }
        }

        /// <summary>
        /// รีเซ็ตสถิติการเล่นทั้งหมดเมื่อเริ่มกะใหม่
        /// </summary>
        public void ResetStats()
        {
            VehiclesAllowedCount = 0;
            VehiclesDeniedCount = 0;
            VehiclesEliminatedCount = 0;
            MistakeCount = 0;
            AnomaliesInfiltratedCount = 0;
            CurrentWave = GameWave.Wave1;
            EndResult = GameEndResult.None;

            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// บันทึกผลการตัดสินใจของผู้เล่นต่อรถ 1 คันตามกฎของ Night Stand
        /// </summary>
        public void RegisterVehicleDecision(VehicleDataSO vehicle, PlayerDecision decision)
        {
            if (vehicle == null || vehicle.NpcData == null)
            {
                Debug.LogWarning("[GameManager] Cannot register decision: VehicleDataSO or NPCDataSO is null.");
                return;
            }

            RegisterDecision(vehicle.NpcData.Status, decision);
        }

        /// <summary>
        /// บันทึกผลการตัดสินใจจากสถานะ NPC โดยตรง
        /// </summary>
        public void RegisterDecision(NPCStatus npcStatus, PlayerDecision decision)
        {
            if (IsGameOver)
            {
                return;
            }

            switch (decision)
            {
                case PlayerDecision.Allow:
                    VehiclesAllowedCount++;
                    if (npcStatus == NPCStatus.Anomaly)
                    {
                        // ปล่อย Anomaly เข้าหมู่บ้าน: เกมดำเนินต่อเพื่อสร้างความระแวง และไปเฉลยตอน 06:00 น.
                        AnomaliesInfiltratedCount++;
                    }
                    else if (npcStatus == NPCStatus.Normal_Invalid)
                    {
                        MistakeCount++;
                    }
                    break;

                case PlayerDecision.Deny:
                    VehiclesDeniedCount++;
                    if (npcStatus == NPCStatus.Normal_Valid)
                    {
                        MistakeCount++;
                    }
                    break;

                case PlayerDecision.Eliminate:
                    VehiclesEliminatedCount++;
                    if (npcStatus != NPCStatus.Anomaly)
                    {
                        // กำจัดคนปกติ (ทั้ง Valid และ Invalid) -> Game Over ทันที
                        OnStatsUpdated?.Invoke();
                        EndGame(GameEndResult.Lose_KilledInnocent);
                        return;
                    }
                    break;
            }

            OnStatsUpdated?.Invoke();
        }

        private void HandleWaveChanged(GameWave newWave)
        {
            CurrentWave = newWave;
        }

        private void HandleShiftEnded()
        {
            if (IsGameOver)
            {
                return;
            }

            GameEndResult result = AnomaliesInfiltratedCount > 0
                ? GameEndResult.Lose_Infiltrated
                : GameEndResult.Win_ShiftComplete;

            EndGame(result);
        }

        private void EndGame(GameEndResult result)
        {
            EndResult = result;

            if (timeWaveManager != null)
            {
                timeWaveManager.PauseTime();
            }

            Debug.Log($"[GameManager] Game Ended: {result} | Allowed: {VehiclesAllowedCount}, Mistakes: {MistakeCount}, Infiltrated: {AnomaliesInfiltratedCount}");
            OnGameEnded?.Invoke(result);
        }
    }
}
