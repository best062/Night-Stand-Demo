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
        [SerializeField] private Vector3 openLocalEuler = new Vector3(0f, 0f, 80f);
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

        public void AllowCurrentVehicle() => AllowCurrentVehicle(bypassPassCheck: false);

        public void AllowCurrentVehicle(bool bypassPassCheck)
        {
            VehicleController vehicle = GetWaitingVehicle();
            if (vehicle == null)
            {
                return;
            }

            if (!bypassPassCheck && !ValidatePassReturned(vehicle))
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterVehicleDecision(vehicle.VehicleData, PlayerDecision.Allow);
            }

            if (ResidentDatabase.Instance != null && vehicle.VehicleData != null)
            {
                ResidentDatabase.Instance.RegisterVehicleEntry(vehicle.VehicleData.FrontLicensePlate);
            }

            isBarrierOpen = true;
            vehicle.OnVehicleExited += HandleAllowedVehicleExited;
            vehicle.AllowPass(exitPoint);
        }

        public void DenyCurrentVehicle() => DenyCurrentVehicle(bypassPassCheck: false);

        public void DenyCurrentVehicle(bool bypassPassCheck)
        {
            VehicleController vehicle = GetWaitingVehicle();
            if (vehicle == null)
            {
                return;
            }

            if (!bypassPassCheck && !ValidatePassReturned(vehicle))
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

        private bool ValidatePassReturned(VehicleController vehicle)
        {
            if (vehicle.CanGateProcessDecision)
            {
                return true;
            }

            string msg = !vehicle.HasDriverHandedPass
                ? "Take the driver's entry pass at the window first!"
                : "Return the entry pass to the driver before opening/closing the gate!";

            Debug.Log($"[GateController] {msg}");
            AIPartnerManager.Instance?.ShowSystemNotice(msg);
            return false;
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
