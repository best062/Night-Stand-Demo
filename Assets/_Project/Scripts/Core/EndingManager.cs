using NightStand.Data;
using NightStand.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NightStand.Core
{
    /// <summary>
    /// จัดการหน้าจอฉากจบทั้ง 3 รูปแบบ เชื่อมต่อกับ GameManager.OnGameEnded
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private FirstPersonController playerController;

        [Header("Unified Ending UI")]
        [SerializeField] private GameObject endingPanelRoot;
        [SerializeField] private TMP_Text endingTitleText;
        [SerializeField] private TMP_Text endingDescriptionText;
        [SerializeField] private TMP_Text statsSummaryText;
        [SerializeField] private Button restartButton;

        [Header("Optional Dedicated Sub-Panels")]
        [SerializeField] private GameObject instantArrestPanel;
        [SerializeField] private GameObject badEndingInfiltratedPanel;
        [SerializeField] private GameObject goodEndingPanel;

        [Header("Ending Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip arrestSirenClip;
        [SerializeField] private AudioClip badEndingClip;
        [SerializeField] private AudioClip goodEndingClip;

        private void Awake()
        {
            if (gameManager == null) gameManager = GameManager.Instance != null ? GameManager.Instance : FindFirstObjectByType<GameManager>();
            if (playerController == null) playerController = FindFirstObjectByType<FirstPersonController>();

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartShift);
            }

            HideAllPanels();
        }

        private void OnEnable()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance != null ? GameManager.Instance : FindFirstObjectByType<GameManager>();
            }

            if (gameManager != null)
            {
                gameManager.OnGameEnded += HandleGameEnded;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnGameEnded -= HandleGameEnded;
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartShift);
            }
        }

        public void RestartShift()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleGameEnded(GameEndResult result)
        {
            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            HideAllPanels();

            if (endingPanelRoot != null)
            {
                endingPanelRoot.SetActive(true);
            }

            switch (result)
            {
                case GameEndResult.Lose_KilledInnocent:
                    ShowInstantArrestEnding();
                    break;
                case GameEndResult.Lose_Infiltrated:
                    ShowBadEnding();
                    break;
                case GameEndResult.Win_ShiftComplete:
                    ShowGoodEnding();
                    break;
            }

            UpdateStatsSummary(result);
        }

        private void ShowInstantArrestEnding()
        {
            if (instantArrestPanel != null) instantArrestPanel.SetActive(true);

            if (endingTitleText != null)
            {
                endingTitleText.text = "GAME OVER: ARRESTED";
                endingTitleText.color = Color.red;
            }

            if (endingDescriptionText != null)
            {
                endingDescriptionText.text =
                    "คุณใช้อุปกรณ์กำจัดกับประชาชนผู้บริสุทธิ์!\n" +
                    "หน่วยงานรัฐบาลบุกเข้าควบคุมตัวคุณทันทีในข้อหาสังหารพลเรือน";
            }

            PlayEndingClip(arrestSirenClip);
        }

        private void ShowBadEnding()
        {
            if (badEndingInfiltratedPanel != null) badEndingInfiltratedPanel.SetActive(true);

            if (endingTitleText != null)
            {
                endingTitleText.text = "BAD ENDING: VILLAGE INFILTRATED";
                endingTitleText.color = new Color(1f, 0.45f, 0.2f);
            }

            if (endingDescriptionText != null)
            {
                int infiltrated = gameManager != null ? gameManager.AnomaliesInfiltratedCount : 0;
                endingDescriptionText.text =
                    $"ครบกำหนดกะ 06:00 น. แล้ว... แต่คุณปล่อย Anomaly หลุดเข้าหมู่บ้านจำนวน {infiltrated} ราย!\n" +
                    "หมู่บ้านถูกแทรกซึมโดยสมบูรณ์ ภารกิจล้มเหลว";
            }

            PlayEndingClip(badEndingClip);
        }

        private void ShowGoodEnding()
        {
            if (goodEndingPanel != null) goodEndingPanel.SetActive(true);

            if (endingTitleText != null)
            {
                endingTitleText.text = "SHIFT COMPLETE: VILLAGE SECURED";
                endingTitleText.color = Color.green;
            }

            if (endingDescriptionText != null)
            {
                string grade = CalculateGrade(gameManager != null ? gameManager.MistakeCount : 0);
                endingDescriptionText.text =
                    "คุณปฏิบัติหน้าที่ครบกะ 06:00 น. โดยไม่มี Anomaly หลุดเข้าหมู่บ้านเลย!\n" +
                    $"ระดับการประเมินผลงาน (Grade): {grade}";
            }

            PlayEndingClip(goodEndingClip);
        }

        private void UpdateStatsSummary(GameEndResult result)
        {
            if (statsSummaryText == null || gameManager == null)
            {
                return;
            }

            statsSummaryText.text =
                $"ผลการคัดกรอง:\n" +
                $"• ปล่อยผ่าน (Allow): {gameManager.VehiclesAllowedCount}\n" +
                $"• ปฏิเสธ (Deny): {gameManager.VehiclesDeniedCount}\n" +
                $"• กำจัด (Eliminate): {gameManager.VehiclesEliminatedCount}\n" +
                $"• ความผิดพลาดเอกสาร (Mistake Count): {gameManager.MistakeCount}\n" +
                $"• Anomaly ที่หลุดเข้าหมู่บ้าน: {gameManager.AnomaliesInfiltratedCount}";
        }

        private string CalculateGrade(int mistakes)
        {
            if (mistakes == 0) return "S";
            if (mistakes <= 1) return "A";
            if (mistakes <= 3) return "B";
            return "C";
        }

        private void HideAllPanels()
        {
            if (endingPanelRoot != null) endingPanelRoot.SetActive(false);
            if (instantArrestPanel != null) instantArrestPanel.SetActive(false);
            if (badEndingInfiltratedPanel != null) badEndingInfiltratedPanel.SetActive(false);
            if (goodEndingPanel != null) goodEndingPanel.SetActive(false);
        }

        private void PlayEndingClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
