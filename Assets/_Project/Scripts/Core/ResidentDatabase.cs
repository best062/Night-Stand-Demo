using System;
using System.Collections.Generic;
using NightStand.Data;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// ระบบฐานข้อมูลลูกบ้านและประวัติการเข้าหมู่บ้านสำหรับตรวจสอบป้ายทะเบียนและจับผิด Doppelgänger
    /// </summary>
    public class ResidentDatabase : MonoBehaviour
    {
        public static ResidentDatabase Instance { get; private set; }

        [Header("Source Document")]
        [SerializeField] private DocumentDataSO residentListDocument;

        // [Data Structure: Dictionary<string, ResidentRecord> - Hash Table for Resident Lookup]
        // Maps license plate (Key) to ResidentRecord (Value) for instant verification.
        // Time Complexity: Build O(n), Lookup TryGetValue O(1) average
        private readonly Dictionary<string, ResidentRecord> residentRegistry =
            new Dictionary<string, ResidentRecord>(StringComparer.OrdinalIgnoreCase);

        // [Data Structure: HashSet<string> - Entered Plates History for Doppelgänger Detection]
        // Stores license plates that have already been allowed into the village during this shift.
        // Time Complexity: Add O(1) average, Contains O(1) average
        private readonly HashSet<string> enteredPlatesHistory =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildDatabase();
        }

        // [Algorithm: Hash Table Population - O(n) where n is number of resident records]
        public void BuildDatabase()
        {
            residentRegistry.Clear();

            if (residentListDocument == null || residentListDocument.ResidentRecords == null)
            {
                return;
            }

            foreach (ResidentRecord record in residentListDocument.ResidentRecords)
            {
                if (!string.IsNullOrWhiteSpace(record.licensePlate))
                {
                    residentRegistry[record.licensePlate.Trim()] = record;
                }
            }
        }

        // [Operation: O(1) Average Lookup in Dictionary<string, ResidentRecord>]
        public bool TryGetResident(string licensePlate, out ResidentRecord record)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                record = default;
                return false;
            }

            return residentRegistry.TryGetValue(licensePlate.Trim(), out record);
        }

        // [Operation: O(1) Average Membership Check in Dictionary]
        public bool IsRegisteredPlate(string licensePlate)
        {
            return !string.IsNullOrWhiteSpace(licensePlate) &&
                   residentRegistry.ContainsKey(licensePlate.Trim());
        }

        // [Operation: O(1) Average Insertion into HashSet<string>]
        public void RegisterVehicleEntry(string licensePlate)
        {
            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                enteredPlatesHistory.Add(licensePlate.Trim());
            }
        }

        // [Operation: O(1) Average Duplicate/Doppelgänger Check in HashSet<string>]
        public bool IsDoppelganger(string licensePlate)
        {
            return !string.IsNullOrWhiteSpace(licensePlate) &&
                   enteredPlatesHistory.Contains(licensePlate.Trim());
        }

        public void ResetHistory()
        {
            enteredPlatesHistory.Clear();
        }
    }
}
