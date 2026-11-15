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

        public event Action<VehicleController> OnArrivedAtGate;
        public event Action<VehicleController> OnVehicleExited;

        public VehicleDataSO VehicleData { get; private set; }
        public VehicleState CurrentState { get; private set; } = VehicleState.Approaching;

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
        }

        private GameWave GetCurrentWave()
        {
            return timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
        }
    }
}
