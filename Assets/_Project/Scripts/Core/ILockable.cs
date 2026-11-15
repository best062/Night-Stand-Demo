using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// Interface สำหรับวัตถุที่ล็อกอยู่และต้องใช้ Key ไขเพื่อเปิด (เช่น ตู้เอกสาร หรือท้ายรถ)
    /// </summary>
    public interface ILockable
    {
        bool IsLocked { get; }
        bool TryUnlock(GameObject user, string keyId = null);
    }
}
