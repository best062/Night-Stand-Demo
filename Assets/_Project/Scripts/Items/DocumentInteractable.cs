using NightStand.Core;
using NightStand.Data;
using NightStand.UI;
using NightStand.Vehicle;
using UnityEngine;

namespace NightStand.Items
{
    /// <summary>
    /// เอกสารที่วางบนโต๊ะป้อมยาม (รายชื่อลูกบ้าน / บัตรผ่าน) กด E เพื่อเปิดดูแบบ 2D UI Overlay
    /// </summary>
    public class DocumentInteractable : MonoBehaviour, IInteractable
    {
        [Header("Document Data")]
        [SerializeField] private DocumentDataSO documentData;
        [SerializeField] private string promptOverride;

        [Header("Dynamic Vehicle Pass (Optional)")]
        [Tooltip("เปิดใช้เมื่อเอกสารชิ้นนี้เป็นบัตรผ่านของรถคันปัจจุบันที่จอดรอหน้าด่าน")]
        [SerializeField] private bool showCurrentVehiclePass = false;
        [SerializeField] private VehicleSpawner vehicleSpawner;

        public string InteractionPrompt
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(promptOverride))
                {
                    return promptOverride;
                }

                return documentData != null ? $"Inspect {documentData.DocumentTitle}" : "Inspect Document";
            }
        }

        private void Awake()
        {
            if (showCurrentVehiclePass && vehicleSpawner == null)
            {
                vehicleSpawner = FindFirstObjectByType<VehicleSpawner>();
            }
        }

        public void SetDocumentData(DocumentDataSO data)
        {
            documentData = data;
        }

        public void Interact(GameObject interactor)
        {
            if (DocumentViewerUI.Instance == null)
            {
                Debug.LogWarning("[DocumentInteractable] DocumentViewerUI.Instance is missing in scene.");
                return;
            }

            if (showCurrentVehiclePass)
            {
                OpenVehiclePassOverlay();
                return;
            }

            if (documentData != null)
            {
                DocumentViewerUI.Instance.OpenDocument(documentData);
            }
            else
            {
                Debug.LogWarning($"[DocumentInteractable] No DocumentDataSO assigned on {name}.");
            }
        }

        private void OpenVehiclePassOverlay()
        {
            if (vehicleSpawner == null)
            {
                vehicleSpawner = FindFirstObjectByType<VehicleSpawner>();
            }

            VehicleController currentVehicle = vehicleSpawner != null ? vehicleSpawner.CurrentVehicle : null;
            if (currentVehicle == null || currentVehicle.VehicleData == null)
            {
                DocumentViewerUI.Instance.OpenCustomDocument(
                    "บัตรผ่านเข้าหมู่บ้าน",
                    "ยังไม่มีรถจอดรอตรวจที่หน้าด่านในขณะนี้",
                    documentData != null ? documentData.DocumentSprite : null
                );
                return;
            }

            VehicleDataSO vData = currentVehicle.VehicleData;
            NPCDataSO npc = vData.NpcData;
            string npcName = npc != null ? npc.NpcName : "-";
            string house = npc != null && !string.IsNullOrWhiteSpace(npc.HouseNumber) ? npc.HouseNumber : "-";

            string passContent =
                $"ชื่อผู้ขอผ่าน: {npcName}\n" +
                $"บ้านเลขที่: {house}\n" +
                $"รุ่นรถ / สี: {vData.VehicleModel} ({vData.VehicleColorName})\n" +
                $"ทะเบียนหน้า: {vData.FrontLicensePlate}\n" +
                $"ทะเบียนหลัง: {vData.RearLicensePlate}";

            DocumentViewerUI.Instance.OpenCustomDocument(
                documentData != null ? documentData.DocumentTitle : "บัตรผ่านเข้าหมู่บ้าน",
                passContent,
                documentData != null ? documentData.DocumentSprite : null
            );
        }
    }
}
