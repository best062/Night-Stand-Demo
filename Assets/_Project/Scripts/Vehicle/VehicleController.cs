using System;
using NightStand.Core;
using NightStand.Data;
using TMPro;
using UnityEngine;

namespace NightStand.Vehicle
{
    public enum VehicleState
    {
        Approaching,
        WaitingAtGate,
        PassingThrough,
        TurningBack,
        Exited
    }

    /// <summary>
    /// ควบคุมการเคลื่อนที่ของรถแต่ละคัน ข้อมูล VehicleDataSO และจุด Waypoint บนตัวรถ
    /// </summary>
    public class VehicleController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Optional Visuals")]
        [SerializeField] private Renderer[] bodyRenderers;
        [SerializeField] private TMP_Text frontPlateText;
        [SerializeField] private TMP_Text rearPlateText;

        [Header("Anomaly Visual Props (Optional)")]
        [SerializeField] private GameObject driverVisual;
        [SerializeField] private GameObject stowawayVisual;
        [SerializeField] private GameObject hitAndRunVisual;
        [SerializeField] private GameObject mimicSlimeVisual;
        [SerializeField] private GameObject trunkBloodVisual;
        [SerializeField] private GameObject clawMarksVisual;
        [SerializeField] private GameObject redHeadlightsVisual;

        public event Action<VehicleController> OnArrivedAtGate;
        public event Action<VehicleController> OnVehicleExited;

        public VehicleDataSO VehicleData { get; private set; }
        public VehicleState CurrentState { get; private set; } = VehicleState.Approaching;

        // [Data Structure: Array (VehicleWaypoint[]) - Fixed-Size Inspection Waypoints]
        // Stores all 4 inspection points around the vehicle (Front, DriverWindow, BackSeat, Trunk).
        // Time Complexity: RefreshWaypoints O(n) where n = number of waypoints on the vehicle (n = 4)
        private VehicleWaypoint[] waypoints;
        private TimeWaveManager timeWaveManager;
        private Vector3 targetPosition;

        private void Awake()
        {
            waypoints = GetComponentsInChildren<VehicleWaypoint>(true);
            timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
        }

        private void OnEnable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged += HandleWaveChanged;
            }
        }

        private void OnDisable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged -= HandleWaveChanged;
            }
        }

        private void Update()
        {
            if (CurrentState == VehicleState.WaitingAtGate || CurrentState == VehicleState.Exited)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                transform.position = targetPosition;
                HandleReachedDestination();
            }
        }

        public void Initialize(VehicleDataSO data, Transform gateStopPoint, GameWave currentWave)
        {
            VehicleData = data;
            CurrentState = VehicleState.Approaching;
            targetPosition = gateStopPoint != null ? gateStopPoint.position : transform.position;

            ApplyVisualData(data);
            RefreshWaypoints(currentWave, isVehicleWaiting: false);
        }

        public void AllowPass(Transform exitPoint)
        {
            if (CurrentState != VehicleState.WaitingAtGate)
            {
                return;
            }

            CurrentState = VehicleState.PassingThrough;
            targetPosition = exitPoint != null ? exitPoint.position : transform.position + transform.forward * 20f;
            RefreshWaypoints(GetCurrentWave(), isVehicleWaiting: false);
        }

        public void DenyEntry(Transform returnPoint)
        {
            if (CurrentState != VehicleState.WaitingAtGate)
            {
                return;
            }

            CurrentState = VehicleState.TurningBack;
            targetPosition = returnPoint != null ? returnPoint.position : transform.position - transform.forward * 20f;
            RefreshWaypoints(GetCurrentWave(), isVehicleWaiting: false);
        }

        /// <summary>
        /// ลบรถออกจากฉากทันทีหลังจากถูก Eliminator กำจัด (เพื่อแจ้ง VehicleSpawner ให้เรียกรถคันถัดไป)
        /// </summary>
        public void EliminateAndRemove()
        {
            if (CurrentState == VehicleState.Exited)
            {
                return;
            }

            CurrentState = VehicleState.Exited;
            RefreshWaypoints(GetCurrentWave(), isVehicleWaiting: false);
            OnVehicleExited?.Invoke(this);
            Destroy(gameObject);
        }

        private void HandleReachedDestination()
        {
            if (CurrentState == VehicleState.Approaching)
            {
                CurrentState = VehicleState.WaitingAtGate;
                RefreshWaypoints(GetCurrentWave(), isVehicleWaiting: true);
                OnArrivedAtGate?.Invoke(this);
            }
            else if (CurrentState == VehicleState.PassingThrough || CurrentState == VehicleState.TurningBack)
            {
                CurrentState = VehicleState.Exited;
                OnVehicleExited?.Invoke(this);
                Destroy(gameObject);
            }
        }

        private void HandleWaveChanged(GameWave newWave)
        {
            if (CurrentState == VehicleState.WaitingAtGate)
            {
                RefreshWaypoints(newWave, isVehicleWaiting: true);
            }
        }

        private void RefreshWaypoints(GameWave wave, bool isVehicleWaiting)
        {
            if (waypoints == null)
            {
                return;
            }

            foreach (VehicleWaypoint wp in waypoints)
            {
                if (wp != null)
                {
                    wp.UpdateVisibility(wave, isVehicleWaiting);
                }
            }
        }

        private void ApplyVisualData(VehicleDataSO data)
        {
            if (data == null)
            {
                return;
            }

            if (bodyRenderers != null)
            {
                foreach (Renderer rend in bodyRenderers)
                {
                    if (rend != null)
                    {
                        rend.material.color = data.VehicleColor;
                    }
                }
            }

            if (frontPlateText != null)
            {
                frontPlateText.text = data.FrontLicensePlate;
            }

            if (rearPlateText != null)
            {
                rearPlateText.text = data.RearLicensePlate;
            }

            ApplyAnomalyVisuals(data);
        }

        private void ApplyAnomalyVisuals(VehicleDataSO data)
        {
            AnomalyDataSO effectiveAnomaly = data.EffectiveAnomalyData;
            string anomalyName = data.IsAnomalyVehicle && effectiveAnomaly != null
                ? effectiveAnomaly.AnomalyName ?? string.Empty
                : string.Empty;

            bool isGhostDriver = anomalyName.IndexOf("Ghost", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isStowaway = anomalyName.IndexOf("Stowaway", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isHitAndRun = anomalyName.IndexOf("Hit", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMimic = anomalyName.IndexOf("Mimic", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isTrunkBlood = anomalyName.IndexOf("Trunk", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isClawMarks = anomalyName.IndexOf("Beast", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               anomalyName.IndexOf("Claw", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isRedHeadlights = anomalyName.IndexOf("Headlight", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   anomalyName.IndexOf("Possessed", StringComparison.OrdinalIgnoreCase) >= 0;

            if (driverVisual != null) driverVisual.SetActive(!isGhostDriver);
            if (stowawayVisual != null) stowawayVisual.SetActive(isStowaway);
            if (hitAndRunVisual != null) hitAndRunVisual.SetActive(isHitAndRun);
            if (mimicSlimeVisual != null) mimicSlimeVisual.SetActive(isMimic);
            if (trunkBloodVisual != null) trunkBloodVisual.SetActive(isTrunkBlood);
            if (clawMarksVisual != null) clawMarksVisual.SetActive(isClawMarks);
            if (redHeadlightsVisual != null) redHeadlightsVisual.SetActive(isRedHeadlights);
        }

        private GameWave GetCurrentWave()
        {
            return timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
        }
    }
}
