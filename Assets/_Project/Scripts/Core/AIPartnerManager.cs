using System;
using System.Collections.Generic;
using NightStand.Data;
using NightStand.Equipment;
using NightStand.Items;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// ระบบเพื่อนร่วมงาน AI (AI Partner) และระบบ Quest (5p):
    /// มอบหมาย Main Quest (อยู่ให้ครบ 06:00 น.) และ Side Quest ระหว่างคืน พร้อมส่งไอเทมมาที่โต๊ะป้อมยามเมื่อทำสำเร็จ
    /// </summary>
    public class AIPartnerManager : MonoBehaviour
    {
        public static AIPartnerManager Instance { get; private set; }

        [Header("Quests")]
        [SerializeField] private QuestDataSO mainQuest;

        // [Data Structure: List<QuestDataSO> - Sequential Side Quest List]
        // Stores side quests assigned by the AI Partner across Waves 1-3.
        // Time Complexity: TriggerSideQuestForWave / FindQuestById O(n) where n = number of side quests
        [SerializeField] private List<QuestDataSO> sideQuests = new List<QuestDataSO>();

        [Header("Dependencies")]
        [SerializeField] private TimeWaveManager timeWaveManager;
        [SerializeField] private DeskItemSpawner deskItemSpawner;
        [SerializeField] private EquipmentManager equipmentManager;

        [Header("Radio Audio (Optional)")]
        [SerializeField] private AudioSource radioAudioSource;
        [SerializeField] private AudioClip radioMessageClip;
        [SerializeField] private AudioClip questCompleteClip;

        public event Action<QuestDataSO> OnQuestStarted;
        public event Action<QuestDataSO> OnQuestCompleted;
        public event Action<string> OnPartnerMessage;

        public QuestDataSO ActiveMainQuest { get; private set; }
        public QuestDataSO ActiveSideQuest { get; private set; }
        public bool IsMainQuestCompleted { get; private set; }

        // [Data Structure: HashSet<string> - Quest State Tracking]
        // Tracks started and completed Quest IDs to prevent duplicate triggers/rewards.
        // Time Complexity: Contains / Add O(1) average
        private readonly HashSet<string> completedQuestIds = new HashSet<string>();
        private readonly HashSet<string> startedQuestIds = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (timeWaveManager == null) timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
            if (deskItemSpawner == null) deskItemSpawner = FindFirstObjectByType<DeskItemSpawner>();
            if (equipmentManager == null) equipmentManager = FindFirstObjectByType<EquipmentManager>();
        }

        private void OnEnable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged += HandleWaveChanged;
                timeWaveManager.OnShiftEnded += HandleShiftEnded;
            }
        }

        private void OnDisable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnWaveChanged -= HandleWaveChanged;
                timeWaveManager.OnShiftEnded -= HandleShiftEnded;
            }
        }

        private void Start()
        {
            if (mainQuest != null)
            {
                StartQuest(mainQuest);
            }

            GameWave initialWave = timeWaveManager != null ? timeWaveManager.CurrentWave : GameWave.Wave1;
            TriggerSideQuestForWave(initialWave);
        }

        public void StartQuest(QuestDataSO quest)
        {
            if (quest == null || startedQuestIds.Contains(quest.QuestId))
            {
                return;
            }

            startedQuestIds.Add(quest.QuestId);

            if (quest.Category == QuestCategory.MainQuest)
            {
                ActiveMainQuest = quest;
                IsMainQuestCompleted = false;
            }
            else
            {
                ActiveSideQuest = quest;
            }

            PlayRadioClip(radioMessageClip);
            OnQuestStarted?.Invoke(quest);

            if (!string.IsNullOrWhiteSpace(quest.PartnerIntroMessage))
            {
                OnPartnerMessage?.Invoke(quest.PartnerIntroMessage);
            }
        }

        public void CompleteQuest(QuestDataSO quest)
        {
            if (quest == null)
            {
                return;
            }

            CompleteQuest(quest.QuestId);
        }

        public void CompleteQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || completedQuestIds.Contains(questId))
            {
                return;
            }

            QuestDataSO quest = FindQuestById(questId);
            if (quest == null)
            {
                Debug.LogWarning($"[AIPartnerManager] Quest ID '{questId}' not found.");
                return;
            }

            completedQuestIds.Add(questId);

            if (quest == ActiveMainQuest)
            {
                IsMainQuestCompleted = true;
            }
            else if (quest == ActiveSideQuest)
            {
                ActiveSideQuest = null;
            }

            PlayRadioClip(questCompleteClip);
            GrantQuestRewards(quest);

            OnQuestCompleted?.Invoke(quest);

            if (!string.IsNullOrWhiteSpace(quest.PartnerCompleteMessage))
            {
                OnPartnerMessage?.Invoke(quest.PartnerCompleteMessage);
            }

            // หากยังมี Side Quest อื่นใน Wave ปัจจุบันที่ยังไม่เริ่ม ให้มอบหมายต่อ
            if (quest.Category == QuestCategory.SideQuest && timeWaveManager != null)
            {
                TriggerSideQuestForWave(timeWaveManager.CurrentWave);
            }
        }

        public bool IsQuestCompleted(string questId)
        {
            return completedQuestIds.Contains(questId);
        }

        private void HandleWaveChanged(GameWave newWave)
        {
            TriggerSideQuestForWave(newWave);
        }

        private void HandleShiftEnded()
        {
            if (ActiveMainQuest != null && !IsMainQuestCompleted)
            {
                CompleteQuest(ActiveMainQuest);
            }
        }

        private void TriggerSideQuestForWave(GameWave wave)
        {
            if (ActiveSideQuest != null)
            {
                return;
            }

            foreach (QuestDataSO quest in sideQuests)
            {
                if (quest == null || startedQuestIds.Contains(quest.QuestId))
                {
                    continue;
                }

                if ((int)quest.TriggerWave <= (int)wave)
                {
                    StartQuest(quest);
                    return;
                }
            }
        }

        private void GrantQuestRewards(QuestDataSO quest)
        {
            if (quest.RewardDeskSpawnIndex >= 0 && deskItemSpawner != null)
            {
                deskItemSpawner.SpawnEntry(quest.RewardDeskSpawnIndex);
            }

            if (quest.RewardEquipmentSlotIndex >= 0 && equipmentManager != null)
            {
                equipmentManager.UnlockSlot(quest.RewardEquipmentSlotIndex, equipImmediately: false);
            }
        }

        private QuestDataSO FindQuestById(string questId)
        {
            if (mainQuest != null && mainQuest.QuestId == questId)
            {
                return mainQuest;
            }

            foreach (QuestDataSO q in sideQuests)
            {
                if (q != null && q.QuestId == questId)
                {
                    return q;
                }
            }

            return null;
        }

        private void PlayRadioClip(AudioClip clip)
        {
            if (radioAudioSource != null && clip != null)
            {
                radioAudioSource.PlayOneShot(clip);
            }
        }
    }
}
