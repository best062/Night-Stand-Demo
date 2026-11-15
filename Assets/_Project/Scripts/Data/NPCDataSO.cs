using System.Collections.Generic;
using UnityEngine;

namespace NightStand.Data
{
    /// <summary>
    /// ข้อมูลของบุคคล (NPC) ที่ขับรถหรือโดยสารเข้าด่านตรวจ
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "Night Stand/Data/NPC Data")]
    public class NPCDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string npcName;
        [SerializeField] private Sprite portrait;

        [Header("Classification")]
        [SerializeField] private NPCStatus status = NPCStatus.Normal_Valid;

        [Tooltip("ลิงก์ข้อมูล Anomaly (ใช้เมื่อ Status เป็น Anomaly)")]
        [SerializeField] private AnomalyDataSO anomalyData;

        [Tooltip("เหตุผลที่เอกสารไม่ผ่าน (ใช้เมื่อ Status เป็น Normal_Invalid)")]
        [TextArea(2, 3)]
        [SerializeField] private string invalidReason;

        [Header("Normal Dialogue")]
        [TextArea(2, 3)]
        [SerializeField] private List<string> normalDialogues = new List<string>();

        [Header("Phase 3 - Structured Dialogue")]
        [SerializeField] private string houseNumber;

        [TextArea(2, 3)]
        [SerializeField] private string greetingDialogue = "สวัสดีครับ ขอผ่านเข้าหมู่บ้านหน่อยครับ";

        [Tooltip("คำตอบเมื่อผู้เล่นถามชื่อ / บ้านเลขที่")]
        [TextArea(2, 3)]
        [SerializeField] private string identityResponse;

        [Tooltip("คำตอบเมื่อผู้เล่นถามจุดประสงค์")]
        [TextArea(2, 3)]
        [SerializeField] private string purposeResponse;

        [Tooltip("คำตอบเรียงตามลำดับเมื่อผู้เล่นถามจี้จุดพิรุธ (กดซ้ำได้)")]
        [TextArea(2, 3)]
        [SerializeField] private List<string> suspiciousResponses = new List<string>();

        [Tooltip("เสียงประกอบเมื่อถามจี้จุดพิรุธจนถึงจุดพีค (เช่น เสียงแตกพร่า/Jumpscare)")]
        [SerializeField] private AudioClip suspiciousJumpscareClip;

        public string NpcName => npcName;
        public Sprite Portrait => portrait;
        public NPCStatus Status => status;
        public AnomalyDataSO AnomalyData => anomalyData;
        public string InvalidReason => invalidReason;
        public IReadOnlyList<string> NormalDialogues => normalDialogues;
        public string HouseNumber => houseNumber;
        public AudioClip SuspiciousJumpscareClip => suspiciousJumpscareClip;

        public bool IsAnomaly => status == NPCStatus.Anomaly;

        public string GetGreeting()
        {
            return !string.IsNullOrWhiteSpace(greetingDialogue)
                ? greetingDialogue
                : "สวัสดีครับ มาขอผ่านเข้าหมู่บ้านครับ";
        }

        public string GetIdentityResponse()
        {
            if (!string.IsNullOrWhiteSpace(identityResponse))
            {
                return identityResponse;
            }

            string namePart = string.IsNullOrWhiteSpace(npcName) ? "ผม" : $"ผมชื่อ {npcName}";
            string housePart = string.IsNullOrWhiteSpace(houseNumber) ? "" : $" อยู่บ้านเลขที่ {houseNumber} ครับ";
            return $"{namePart}{housePart}";
        }

        public string GetPurposeResponse()
        {
            if (!string.IsNullOrWhiteSpace(purposeResponse))
            {
                return purposeResponse;
            }

            if (normalDialogues != null && normalDialogues.Count > 0)
            {
                return normalDialogues[0];
            }

            return "เพิ่งกลับจากทำธุระข้างนอกครับ จะเข้าบ้านพักผ่อน";
        }

        public string GetSuspiciousResponse(int pressCount)
        {
            if (suspiciousResponses != null && suspiciousResponses.Count > 0)
            {
                int index = Mathf.Clamp(pressCount - 1, 0, suspiciousResponses.Count - 1);
                return suspiciousResponses[index];
            }

            if (IsAnomaly && anomalyData != null && anomalyData.AbnormalDialogues != null && anomalyData.AbnormalDialogues.Count > 0)
            {
                int index = Mathf.Clamp(pressCount - 1, 0, anomalyData.AbnormalDialogues.Count - 1);
                return anomalyData.AbnormalDialogues[index];
            }

            return "เอ่อ... ไม่มีอะไรผิดปกตินี่ครับ รีบเปิดไม้กั้นให้ผมเถอะ";
        }
    }
}
