using NightStand.Core;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Vehicle
{
    public enum VehicleWaypointType
    {
        Front,
        DriverWindow,
        BackSeat,
        Trunk
    }

    /// <summary>
    /// จุดตรวจบนตัวรถ พร้อมระบบแสดง/ซ่อนกรอบนำสายตา (Marker) แบบ Dynamic ตาม Wave
    /// </summary>
    public class VehicleWaypoint : MonoBehaviour, IInteractable
    {
        [Header("Waypoint Config")]
        [SerializeField] private VehicleWaypointType waypointType = VehicleWaypointType.Front;

        [Tooltip("จุดพื้นฐานจะแสดงใน Wave 1 และถูกซ่อนอัตโนมัติใน Wave 2-3 เพื่อให้ผู้เล่นจำเอง")]
        [SerializeField] private bool isBasicWaypoint = true;

        [Tooltip("Wave ที่จุดตรวจใหม่นี้จะปลดล็อกและแสดงกรอบนำสายตา (ใช้เมื่อไม่ใช่จุดพื้นฐาน)")]
        [SerializeField] private GameWave unlockWave = GameWave.Wave2;

        [Header("Visual Marker")]
        [SerializeField] private GameObject markerVisual;
        [SerializeField] private bool billboardToCamera = true;

        private VehicleController ownerVehicle;
        private Collider waypointCollider;
        private Camera mainCamera;

        public VehicleWaypointType WaypointType => waypointType;

        public string InteractionPrompt => $"Inspect {GetDisplayLabel()}";

        private void Awake()
        {
            ownerVehicle = GetComponentInParent<VehicleController>();
            waypointCollider = GetComponent<Collider>();
            mainCamera = Camera.main;

            if (markerVisual != null)
            {
                markerVisual.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (!billboardToCamera || markerVisual == null || !markerVisual.activeSelf)
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                markerVisual.transform.LookAt(
                    markerVisual.transform.position + mainCamera.transform.forward,
                    mainCamera.transform.up
                );
            }
        }

        /// <summary>
        /// อัปเดตการแสดงผลกรอบนำสายตาตาม Wave ปัจจุบันและสถานะการจอดของรถ
        /// </summary>
        public void UpdateVisibility(GameWave currentWave, bool isVehicleWaiting)
        {
            if (waypointCollider != null)
            {
                waypointCollider.enabled = isVehicleWaiting;
            }

            if (markerVisual == null)
            {
                return;
            }

            if (!isVehicleWaiting)
            {
                markerVisual.SetActive(false);
                return;
            }

            bool shouldShowMarker = isBasicWaypoint
                ? currentWave == GameWave.Wave1
                : currentWave == unlockWave;

            markerVisual.SetActive(shouldShowMarker);
        }

        public void Interact(GameObject interactor)
        {
            if (ownerVehicle == null || ownerVehicle.VehicleData == null)
            {
                return;
            }

            VehicleDataSO data = ownerVehicle.VehicleData;
            NPCDataSO npc = data.NpcData;

            string report = $"[Inspect {GetDisplayLabel()}] ";

            switch (waypointType)
            {
                case VehicleWaypointType.Front:
                    report += $"Front Plate: {data.FrontLicensePlate} | Model: {data.VehicleModel} ({data.VehicleColorName})";
                    break;
                case VehicleWaypointType.DriverWindow:
                    report += npc != null
                        ? $"Driver: {npc.NpcName}"
                        : "Driver seat is empty.";
                    break;
                case VehicleWaypointType.BackSeat:
                    report += "Checking back seat...";
                    break;
                case VehicleWaypointType.Trunk:
                    report += $"Rear Plate: {data.RearLicensePlate} | Checking trunk...";
                    break;
            }

            if (npc != null && npc.IsAnomaly && npc.AnomalyData != null && !string.IsNullOrEmpty(npc.AnomalyData.VehicleClue))
            {
                report += $" | Clue Note: {npc.AnomalyData.VehicleClue}";
            }

            Debug.Log(report);
        }

        private string GetDisplayLabel()
        {
            return waypointType switch
            {
                VehicleWaypointType.Front => "Front / Plate",
                VehicleWaypointType.DriverWindow => "Driver Window",
                VehicleWaypointType.BackSeat => "Back Seat",
                VehicleWaypointType.Trunk => "Trunk / Rear Plate",
                _ => waypointType.ToString()
            };
        }
    }
}
