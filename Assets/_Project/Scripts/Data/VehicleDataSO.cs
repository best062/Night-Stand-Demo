using System;
using UnityEngine;

namespace NightStand.Data
{
    /// <summary>
    /// ข้อมูลรถยนต์ที่เข้าด่านตรวจ โดยบรรจุอ้างอิง NPCDataSO ตามข้อกำหนด Class Diagram
    /// </summary>
    [CreateAssetMenu(fileName = "NewVehicleData", menuName = "Night Stand/Data/Vehicle Data")]
    public class VehicleDataSO : ScriptableObject
    {
        [Header("License Plates")]
        [SerializeField] private string frontLicensePlate;
        [SerializeField] private string rearLicensePlate;

        [Header("Vehicle Appearance")]
        [SerializeField] private string vehicleModel;
        [SerializeField] private string vehicleColorName;
        [SerializeField] private Color vehicleColor = Color.white;

        [Header("Occupant (Class Diagram Link)")]
        [Tooltip("ข้อมูล NPC คนขับ/ผู้โดยสารที่ผูกกับรถคันนี้")]
        [SerializeField] private NPCDataSO npcData;

        public string FrontLicensePlate => frontLicensePlate;
        public string RearLicensePlate => rearLicensePlate;
        public string VehicleModel => vehicleModel;
        public string VehicleColorName => vehicleColorName;
        public Color VehicleColor => vehicleColor;
        public NPCDataSO NpcData => npcData;

        public bool HasMatchingPlates =>
            string.Equals(frontLicensePlate?.Trim(), rearLicensePlate?.Trim(), StringComparison.OrdinalIgnoreCase);

        public bool IsAnomalyVehicle => npcData != null && npcData.IsAnomaly;
    }
}
