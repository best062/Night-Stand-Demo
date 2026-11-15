using System.Collections.Generic;
using UnityEngine;

namespace NightStand.Data
{
    /// <summary>
    /// ข้อมูลของ Anomaly แต่ละประเภทสำหรับใช้ในระบบ Data-Driven
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnomalyData", menuName = "Night Stand/Data/Anomaly Data")]
    public class AnomalyDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string anomalyName;
        [SerializeField] private GameWave appearWave = GameWave.Wave1;

        [Header("Investigation Clues")]
        [Tooltip("จุดสังเกตความผิดปกติบนตัวรถหรือรอบรถ")]
        [TextArea(2, 4)]
        [SerializeField] private string vehicleClue;

        [Header("Dialogue")]
        [Tooltip("บทสนทนาที่ผิดปกติหรือน่าสงสัยเมื่อผู้เล่นซักถาม NPC")]
        [TextArea(2, 3)]
        [SerializeField] private List<string> abnormalDialogues = new List<string>();

        public string AnomalyName => anomalyName;
        public GameWave AppearWave => appearWave;
        public string VehicleClue => vehicleClue;
        public IReadOnlyList<string> AbnormalDialogues => abnormalDialogues;
    }
}
