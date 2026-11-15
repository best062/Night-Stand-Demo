using NightStand.Core;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Vehicle
{
    /// <summary>
    /// ควบคุมไม้กั้นหน้าป้อมยาม ส่งรถผ่านเข้าหมู่บ้านหรือไล่กลับ และอัปเดตสถิติไปยัง GameManager
    /// </summary>
    public class GateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private VehicleSpawner vehicleSpawner;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Transform returnPoint;

        [Header("Barrier Arm Visual (Optional)")]
        [SerializeField] private Transform barrierArm;
        [SerializeField] private Vector3 closedLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 openLocalEuler = new Vector3(0f, 0f, -80f);
        [SerializeField] private float barrierRotateSpeed = 180f;

        private bool isBarrierOpen;

        private void Awake()
        {
            if (vehicleSpawner == null)
            {
                vehicleSpawner = FindFirstObjectByType<VehicleSpawner>();
            }
        }

        private void Update()
        {
            if (barrierArm == null)
            {
                return;
            }

            Quaternion targetRot = Quaternion.Euler(isBarrierOpen ? openLocalEuler : closedLocalEuler);
            barrierArm.localRotation = Quaternion.RotateTowards(
                barrierArm.localRotation,
                targetRot,
                barrierRotateSpeed * Time.deltaTime
            );
        }

        public void AllowCurrentVehicle()
        {
            VehicleController vehicle = GetWaitingVehicle();
            if (vehicle == null)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterVehicleDecision(vehicle.VehicleData, PlayerDecision.Allow);
            }

            isBarrierOpen = true;
            vehicle.OnVehicleExited += HandleAllowedVehicleExited;
            vehicle.AllowPass(exitPoint);
        }

        public void DenyCurrentVehicle()
        {
            VehicleController vehicle = GetWaitingVehicle();
            if (vehicle == null)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterVehicleDecision(vehicle.VehicleData, PlayerDecision.Deny);
            }

            isBarrierOpen = false;
            vehicle.DenyEntry(returnPoint);
        }

        private VehicleController GetWaitingVehicle()
        {
            if (vehicleSpawner == null || vehicleSpawner.CurrentVehicle == null)
            {
                return null;
            }

            VehicleController vehicle = vehicleSpawner.CurrentVehicle;
            return vehicle.CurrentState == VehicleState.WaitingAtGate ? vehicle : null;
        }

        private void HandleAllowedVehicleExited(VehicleController vehicle)
        {
            if (vehicle != null)
            {
                vehicle.OnVehicleExited -= HandleAllowedVehicleExited;
            }

            isBarrierOpen = false;
        }
    }
}
