using System.Collections;
using NightStand.Core;
using NightStand.Data;
using TMPro;
using UnityEngine;

namespace NightStand.UI
{
    /// <summary>
    /// แสดงผลภารกิจ Main Quest / Side Quest และข้อความวิทยุจากเพื่อนร่วมงาน AI
    /// </summary>
    public class AIPartnerUI : MonoBehaviour
    {
        [Header("Quest HUD")]
        [SerializeField] private TMP_Text mainQuestText;
        [SerializeField] private TMP_Text sideQuestText;

        [Header("AI Partner Radio Banner")]
        [SerializeField] private GameObject radioBannerRoot;
        [SerializeField] private TMP_Text radioMessageText;
        [SerializeField] private float bannerDisplayDuration = 6f;

        [Header("Dependencies")]
        [SerializeField] private AIPartnerManager aiPartnerManager;

        private Coroutine hideBannerCoroutine;

        private void Awake()
        {
            if (aiPartnerManager == null)
            {
                aiPartnerManager = FindFirstObjectByType<AIPartnerManager>();
            }

            if (radioBannerRoot != null)
            {
                radioBannerRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (aiPartnerManager != null)
            {
                aiPartnerManager.OnQuestStarted += HandleQuestUpdated;
                aiPartnerManager.OnQuestCompleted += HandleQuestUpdated;
                aiPartnerManager.OnPartnerMessage += HandlePartnerMessage;
            }
        }

        private void OnDisable()
        {
            if (aiPartnerManager != null)
            {
                aiPartnerManager.OnQuestStarted -= HandleQuestUpdated;
                aiPartnerManager.OnQuestCompleted -= HandleQuestUpdated;
                aiPartnerManager.OnPartnerMessage -= HandlePartnerMessage;
            }
        }

        private void Start()
        {
            RefreshQuestHUD();
        }

        private void HandleQuestUpdated(QuestDataSO _)
        {
            RefreshQuestHUD();
        }

        private void RefreshQuestHUD()
        {
            if (aiPartnerManager == null)
            {
                return;
            }

            if (mainQuestText != null)
            {
                QuestDataSO main = aiPartnerManager.ActiveMainQuest;
                if (main != null)
                {
                    string status = aiPartnerManager.IsMainQuestCompleted ? "[สำเร็จ]" : "[กำลังทำ]";
                    mainQuestText.text = $"<b>Main Quest {status}:</b> {main.QuestTitle}\n<size=85%>{main.Description}</size>";
                }
                else
                {
                    mainQuestText.text = string.Empty;
                }
            }

            if (sideQuestText != null)
            {
                QuestDataSO side = aiPartnerManager.ActiveSideQuest;
                if (side != null)
                {
                    sideQuestText.gameObject.SetActive(true);
                    sideQuestText.text = $"<b>Side Quest:</b> {side.QuestTitle}\n<size=85%>{side.Description}</size>";
                }
                else
                {
                    sideQuestText.text = "<b>Side Quest:</b> ไม่มีภารกิจเสริมในขณะนี้";
                }
            }
        }

        private void HandlePartnerMessage(string message)
        {
            if (radioMessageText != null)
            {
                radioMessageText.text = $"[AI Partner]: {message}";
            }

            if (radioBannerRoot != null)
            {
                radioBannerRoot.SetActive(true);
            }

            if (hideBannerCoroutine != null)
            {
                StopCoroutine(hideBannerCoroutine);
            }

            hideBannerCoroutine = StartCoroutine(HideBannerAfterDelay());
        }

        private IEnumerator HideBannerAfterDelay()
        {
            yield return new WaitForSeconds(bannerDisplayDuration);

            if (radioBannerRoot != null)
            {
                radioBannerRoot.SetActive(false);
            }

            hideBannerCoroutine = null;
        }
    }
}
