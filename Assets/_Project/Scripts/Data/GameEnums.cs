namespace NightStand.Data
{
    /// <summary>
    /// ช่วงเวลาของแต่ละ Wave ใน 1 กะ (18:00 - 06:00)
    /// </summary>
    public enum GameWave
    {
        Wave1 = 1, // 18:00 - 22:00
        Wave2 = 2, // 22:00 - 02:00
        Wave3 = 3  // 02:00 - 06:00
    }

    /// <summary>
    /// สถานะของบุคคล (NPC) ที่ขอผ่านเข้าหมู่บ้าน
    /// </summary>
    public enum NPCStatus
    {
        Normal_Valid,
        Normal_Invalid,
        Anomaly
    }

    /// <summary>
    /// การตัดสินใจของผู้เล่นต่อรถ/บุคคลที่เข้าด่าน
    /// </summary>
    public enum PlayerDecision
    {
        Allow,
        Deny,
        Eliminate
    }

    /// <summary>
    /// ผลลัพธ์เมื่อจบเกมหรือจบกะ
    /// </summary>
    public enum GameEndResult
    {
        None,
        Win_ShiftComplete,
        Lose_Infiltrated,
        Lose_KilledInnocent
    }

    /// <summary>
    /// ประเภทของไอเทมและอุปกรณ์ในเกม (Item 1p)
    /// </summary>
    public enum ItemType
    {
        Document,
        Flashlight,
        Key,
        Eliminator
    }

    /// <summary>
    /// ประเภทของเอกสารหลักฐานบนโต๊ะป้อมยาม
    /// </summary>
    public enum DocumentType
    {
        ResidentList,
        GatePass,
        Memo
    }

    /// <summary>
    /// หมวดหมู่ของ Anomaly (คนผิดปกติ / ตัวรถผิดปกติ / ทั้งคู่)
    /// </summary>
    public enum AnomalyCategory
    {
        None,
        NPC_Anomaly,
        Vehicle_Anomaly,
        Hybrid_Anomaly
    }
}
