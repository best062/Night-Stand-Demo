using UnityEngine;

namespace NightStand.Data
{
    public enum QuestCategory
    {
        MainQuest,
        SideQuest
    }

    /// <summary>
    /// ข้อมูลภารกิจที่มอบหมายโดยเพื่อนร่วมงาน AI (Quest 5p)
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestData", menuName = "Night Stand/Data/Quest Data")]
    public class QuestDataSO : ScriptableObject
    {
        [Header("Quest Identity")]
        [SerializeField] private string questId;
        [SerializeField] private string questTitle = "New Quest";
        [SerializeField] private QuestCategory category = QuestCategory.SideQuest;
        [SerializeField] private GameWave triggerWave = GameWave.Wave1;

        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("AI Partner Radio Messages")]
        [TextArea(2, 3)]
        [SerializeField] private string partnerIntroMessage;

        [TextArea(2, 3)]
        [SerializeField] private string partnerCompleteMessage;

        [Header("Rewards (Item Spawn / Equipment Unlock)")]
        [Tooltip("Index ของ Entry ใน DeskItemSpawner ที่จะเสกของรางวัลเมื่อทำสำเร็จ (-1 = ไม่เสก)")]
        [SerializeField] private int rewardDeskSpawnIndex = -1;

        [Tooltip("Index ของ Slot ใน EquipmentManager ที่จะปลดล็อกทันทีเมื่อทำสำเร็จ (-1 = ไม่ปลดล็อกอัตโนมัติ)")]
        [SerializeField] private int rewardEquipmentSlotIndex = -1;

        public string QuestId => string.IsNullOrWhiteSpace(questId) ? name : questId;
        public string QuestTitle => questTitle;
        public QuestCategory Category => category;
        public GameWave TriggerWave => triggerWave;
        public string Description => description;
        public string PartnerIntroMessage => partnerIntroMessage;
        public string PartnerCompleteMessage => partnerCompleteMessage;
        public int RewardDeskSpawnIndex => rewardDeskSpawnIndex;
        public int RewardEquipmentSlotIndex => rewardEquipmentSlotIndex;
    }
}
