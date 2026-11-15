using NightStand.Data;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// ตัวช่วยสำหรับผูกกับ UnityEvent (เช่น LockedTarget.onUnlocked หรือ Trigger ในฉาก)
    /// เพื่อแจ้ง AIPartnerManager ว่าภารกิจสำเร็จแล้ว
    /// </summary>
    public class QuestObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] private QuestDataSO targetQuest;
        [SerializeField] private string fallbackQuestId;

        public void CompleteTargetQuest()
        {
            if (AIPartnerManager.Instance == null)
            {
                return;
            }

            if (targetQuest != null)
            {
                AIPartnerManager.Instance.CompleteQuest(targetQuest);
            }
            else if (!string.IsNullOrWhiteSpace(fallbackQuestId))
            {
                AIPartnerManager.Instance.CompleteQuest(fallbackQuestId);
            }
        }
    }
}
