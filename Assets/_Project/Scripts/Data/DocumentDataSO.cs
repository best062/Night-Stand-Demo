using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NightStand.Data
{
    [Serializable]
    public struct ResidentRecord
    {
        public string residentName;
        public string houseNumber;
        public string licensePlate;
        public string vehicleInfo;
    }

    /// <summary>
    /// ข้อมูลเอกสารบนโต๊ะป้อมยาม (รายชื่อลูกบ้าน, บัตรผ่าน, บันทึก) สำหรับแสดงบน 2D UI Overlay
    /// </summary>
    [CreateAssetMenu(fileName = "NewDocumentData", menuName = "Night Stand/Data/Document Data")]
    public class DocumentDataSO : ScriptableObject
    {
        [Header("Document Info")]
        [SerializeField] private string documentTitle = "เอกสารตรวจสอบ";
        [SerializeField] private DocumentType documentType = DocumentType.ResidentList;
        [SerializeField] private Sprite documentSprite;

        [Header("Content")]
        [TextArea(4, 10)]
        [SerializeField] private string bodyText;

        [Header("Resident Records (Optional - สำหรับ ResidentList)")]
        [SerializeField] private List<ResidentRecord> residentRecords = new List<ResidentRecord>();

        public string DocumentTitle => documentTitle;
        public DocumentType DocumentType => documentType;
        public Sprite DocumentSprite => documentSprite;
        public string BodyText => bodyText;
        public IReadOnlyList<ResidentRecord> ResidentRecords => residentRecords;

        /// <summary>
        /// รวมข้อความสำหรับแสดงผลบน DocumentViewerUI
        /// </summary>
        public string GetFormattedContent()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(bodyText))
            {
                sb.AppendLine(bodyText.Trim());
            }

            if (residentRecords != null && residentRecords.Count > 0)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                foreach (ResidentRecord record in residentRecords)
                {
                    sb.AppendLine($"• บ้านเลขที่ {record.houseNumber} | {record.residentName} | ทะเบียน: {record.licensePlate} ({record.vehicleInfo})");
                }
            }

            return sb.ToString().Trim();
        }
    }
}
