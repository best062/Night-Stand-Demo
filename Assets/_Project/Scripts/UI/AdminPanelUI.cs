using System;
using NightStand.Core;
using NightStand.Data;
using NightStand.Equipment;
using NightStand.Player;
using NightStand.Vehicle;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NightStand.UI
{
    /// <summary>
    /// หน้าต่าง Admin / Debug Control Panel (กด F1 เพื่อเปิด/ปิด)
    /// สำหรับปรับเวลา Wave, เสกรถแต่ละประเภท, ปลดล็อกอุปกรณ์, ทดสอบ Jumpscare และทริกเกอร์ฉากจบได้ทันที
    /// </summary>
    public class AdminPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text liveStatusText;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private Button closeButton;

        [Header("Dependencies (Auto-find if empty)")]
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private TimeWaveManager timeWaveManager;
        [SerializeField] private VehicleSpawner vehicleSpawner;
        [SerializeField] private GateController gateController;
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private AIPartnerManager aiPartnerManager;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private float statusRefreshTimer;

        private void Awake()
        {
            if (playerController == null) playerController = FindFirstObjectByType<FirstPersonController>();
            if (timeWaveManager == null) timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
            if (vehicleSpawner == null) vehicleSpawner = FindFirstObjectByType<VehicleSpawner>();
            if (gateController == null) gateController = FindFirstObjectByType<GateController>();
            if (equipmentManager == null) equipmentManager = FindFirstObjectByType<EquipmentManager>();
            if (aiPartnerManager == null) aiPartnerManager = FindFirstObjectByType<AIPartnerManager>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => SetOpen(false));
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Start()
        {
            BuildAdminButtons();
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                (Keyboard.current.f1Key.wasPressedThisFrame || Keyboard.current.backquoteKey.wasPressedThisFrame))
            {
                SetOpen(!IsOpen);
            }

            if (IsOpen)
            {
                statusRefreshTimer -= Time.unscaledDeltaTime;
                if (statusRefreshTimer <= 0f)
                {
                    statusRefreshTimer = 0.2f;
                    RefreshLiveStatus();
                }
            }
        }

        public void SetOpen(bool open)
        {
            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(open);

            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(!open);
            }
            else
            {
                Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = open;
            }

            if (open)
            {
                RefreshLiveStatus();
            }
        }

        private void BuildAdminButtons()
        {
            if (buttonContainer == null)
            {
                return;
            }

            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }

            // Time & Wave
            AddActionButton("Wave 1 (18:00)", () => timeWaveManager?.AdminSetWave(GameWave.Wave1));
            AddActionButton("Wave 2 (22:00)", () => timeWaveManager?.AdminSetWave(GameWave.Wave2));
            AddActionButton("Wave 3 (02:00)", () => timeWaveManager?.AdminSetWave(GameWave.Wave3));
            AddActionButton("Jump 05:55 AM", () => timeWaveManager?.AdminJumpNearShiftEnd());
            AddActionButton("Speed 1x", () => timeWaveManager?.AdminSetTimeScale(1f));
            AddActionButton("Speed 5x", () => timeWaveManager?.AdminSetTimeScale(5f));
            AddActionButton("Pause Time", () => timeWaveManager?.PauseTime());
            AddActionButton("Resume Time", () => timeWaveManager?.ResumeTime());

            // Vehicle Spawner & Gate
            AddActionButton("Spawn Normal (Valid)", () => vehicleSpawner?.AdminSpawnVehicleOfStatus(NPCStatus.Normal_Valid));
            AddActionButton("Spawn Normal (Invalid)", () => vehicleSpawner?.AdminSpawnVehicleOfStatus(NPCStatus.Normal_Invalid));
            AddActionButton("Spawn Anomaly", () => vehicleSpawner?.AdminSpawnVehicleOfStatus(NPCStatus.Anomaly));
            AddActionButton("Force Allow Gate", () => gateController?.AllowCurrentVehicle(bypassPassCheck: true));
            AddActionButton("Force Deny Gate", () => gateController?.DenyCurrentVehicle(bypassPassCheck: true));

            // Equipment & Quests
            AddActionButton("Unlock All Gear", UnlockAllEquipment);
            AddActionButton("Complete Side Quest", CompleteCurrentSideQuest);
            AddActionButton("Test Jumpscare", () => JumpscareManager.Instance?.TriggerJumpscare());

            // Endings & Reset
            AddActionButton("End: Arrest (Lose)", () => TriggerEnding(GameEndResult.Lose_KilledInnocent));
            AddActionButton("End: Infiltrated (Lose)", () => TriggerEnding(GameEndResult.Lose_Infiltrated));
            AddActionButton("End: Shift Win", () => TriggerEnding(GameEndResult.Win_ShiftComplete));
            AddActionButton("Restart Scene", () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }

        private void UnlockAllEquipment()
        {
            if (equipmentManager == null) return;
            equipmentManager.UnlockSlot(0, false);
            equipmentManager.UnlockSlot(1, false);
            equipmentManager.UnlockSlot(2, true);
        }

        private void CompleteCurrentSideQuest()
        {
            if (aiPartnerManager != null && aiPartnerManager.ActiveSideQuest != null)
            {
                aiPartnerManager.CompleteQuest(aiPartnerManager.ActiveSideQuest);
            }
        }

        private void TriggerEnding(GameEndResult result)
        {
            SetOpen(false);
            GameManager.Instance?.AdminTriggerEnding(result);
        }

        private void RefreshLiveStatus()
        {
            if (liveStatusText == null)
            {
                return;
            }

            string timeStr = timeWaveManager != null ? $"{timeWaveManager.FormattedTime} ({timeWaveManager.CurrentWave})" : "N/A";
            string queueStr = vehicleSpawner != null ? vehicleSpawner.QueuedCount.ToString() : "0";

            string vehicleInfo = "None";
            if (vehicleSpawner != null && vehicleSpawner.CurrentVehicle != null && vehicleSpawner.CurrentVehicle.VehicleData != null)
            {
                VehicleDataSO v = vehicleSpawner.CurrentVehicle.VehicleData;
                NPCDataSO npc = v.NpcData;
                string npcName = npc != null ? npc.NpcName : "Unknown";
                string status = v.EffectiveStatus.ToString();
                string category = v.IsAnomalyVehicle ? v.AnomalyCategory.ToString() : "Normal";
                string anomaly = v.IsAnomalyVehicle && v.EffectiveAnomalyData != null ? v.EffectiveAnomalyData.AnomalyName : "-";
                vehicleInfo = $"{v.VehicleColorName} {v.VehicleModel} | Front: {v.FrontLicensePlate} | Rear: {v.RearLicensePlate}\n" +
                              $"Driver: {npcName} | Type: {status} ({category}) | Anomaly: {anomaly}";
            }

            string statsInfo = GameManager.Instance != null
                ? $"Allow: {GameManager.Instance.VehiclesAllowedCount} | Deny: {GameManager.Instance.VehiclesDeniedCount} | Elim: {GameManager.Instance.VehiclesEliminatedCount} | Mistakes: {GameManager.Instance.MistakeCount} | Infiltrated: {GameManager.Instance.AnomaliesInfiltratedCount}"
                : "GameManager N/A";

            liveStatusText.text =
                $"<b>[ADMIN INSPECTOR]</b> Time: {timeStr} | Queue Buffer: {queueStr}\n" +
                $"<b>Current Vehicle:</b> {vehicleInfo}\n" +
                $"<b>Stats:</b> {statsInfo}";
        }

        private void AddActionButton(string label, Action onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(buttonContainer, false);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.22f, 0.27f, 0.35f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(4f, 2f);
            rt.offsetMax = new Vector2(-4f, -2f);
        }
    }
}
