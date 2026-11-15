using System;
using NightStand.Data;
using UnityEngine;
using UnityEngine.Events;

namespace NightStand.Core
{
    /// <summary>
    /// จัดการเวลาในเกม (18:00 - 06:00 น.) ภายในเวลาจริง 5.5 นาที และแจ้งเตือนเมื่อเปลี่ยน Wave
    /// </summary>
    public class TimeWaveManager : MonoBehaviour
    {
        private const int StartHour = 18;
        private const float TotalInGameMinutes = 12f * 60f; // 18:00 -> 06:00 = 720 นาทีในเกม
        private const float Wave2StartInGameMinutes = 4f * 60f; // 22:00 (ผ่านไป 4 ชม.)
        private const float Wave3StartInGameMinutes = 8f * 60f; // 02:00 (ผ่านไป 8 ชม.)

        [Header("Shift Settings")]
        [Tooltip("ระยะเวลา 1 กะในเวลาจริง (วินาที) ค่าเริ่มต้น 5.5 นาที = 330 วินาที")]
        [SerializeField] private float shiftDurationSeconds = 330f;

        [SerializeField] private bool autoStartOnPlay = true;

        [Header("Debug / Testing")]
        [Tooltip("ตัวคูณความเร็วเวลาสำหรับทดสอบ (ค่าปกติ = 1)")]
        [SerializeField, Min(0.1f)] private float debugTimeScale = 1f;

        [Header("Inspector UnityEvents (Optional)")]
        [SerializeField] private UnityEvent onWave1Started;
        [SerializeField] private UnityEvent onWave2Started;
        [SerializeField] private UnityEvent onWave3Started;
        [SerializeField] private UnityEvent onShiftEnded;

        public event Action<GameWave> OnWaveChanged;
        public event Action<int, int> OnTimeUpdated;
        public event Action OnShiftEnded;

        public GameWave CurrentWave { get; private set; } = GameWave.Wave1;
        public int CurrentHour { get; private set; } = StartHour;
        public int CurrentMinute { get; private set; } = 0;
        public bool IsRunning { get; private set; }
        public bool HasShiftEnded { get; private set; }
        public float NormalizedProgress => shiftDurationSeconds > 0f
            ? Mathf.Clamp01(elapsedRealSeconds / shiftDurationSeconds)
            : 1f;

        public string FormattedTime => $"{CurrentHour:00}:{CurrentMinute:00}";

        private float elapsedRealSeconds;
        private int lastReportedTotalMinute = -1;

        private void Start()
        {
            if (autoStartOnPlay)
            {
                StartShift();
            }
            else
            {
                UpdateTimeState(0f);
            }
        }

        private void Update()
        {
            if (!IsRunning || HasShiftEnded)
            {
                return;
            }

            elapsedRealSeconds += Time.deltaTime * debugTimeScale;
            float progress = NormalizedProgress;
            float inGameMinutesElapsed = progress * TotalInGameMinutes;

            UpdateTimeState(inGameMinutesElapsed);
            EvaluateWaveTransition(inGameMinutesElapsed);

            if (progress >= 1f)
            {
                CompleteShift();
            }
        }

        /// <summary>
        /// เริ่มต้นกะใหม่ตั้งแต่เวลา 18:00 น. (Wave 1)
        /// </summary>
        public void StartShift()
        {
            elapsedRealSeconds = 0f;
            lastReportedTotalMinute = -1;
            HasShiftEnded = false;
            IsRunning = true;

            UpdateTimeState(0f);
            SetWave(GameWave.Wave1, forceNotify: true);
        }

        public void PauseTime()
        {
            IsRunning = false;
        }

        public void ResumeTime()
        {
            if (!HasShiftEnded)
            {
                IsRunning = true;
            }
        }

        private void UpdateTimeState(float inGameMinutesElapsed)
        {
            int totalMinuteInt = Mathf.Clamp(Mathf.FloorToInt(inGameMinutesElapsed), 0, (int)TotalInGameMinutes);
            if (totalMinuteInt == lastReportedTotalMinute)
            {
                return;
            }

            lastReportedTotalMinute = totalMinuteInt;
            int minutesFromMidnight = ((StartHour * 60) + totalMinuteInt) % (24 * 60);
            CurrentHour = minutesFromMidnight / 60;
            CurrentMinute = minutesFromMidnight % 60;

            OnTimeUpdated?.Invoke(CurrentHour, CurrentMinute);
        }

        private void EvaluateWaveTransition(float inGameMinutesElapsed)
        {
            GameWave targetWave;
            if (inGameMinutesElapsed >= Wave3StartInGameMinutes)
            {
                targetWave = GameWave.Wave3;
            }
            else if (inGameMinutesElapsed >= Wave2StartInGameMinutes)
            {
                targetWave = GameWave.Wave2;
            }
            else
            {
                targetWave = GameWave.Wave1;
            }

            if (targetWave != CurrentWave)
            {
                SetWave(targetWave, forceNotify: false);
            }
        }

        private void SetWave(GameWave wave, bool forceNotify)
        {
            if (!forceNotify && CurrentWave == wave)
            {
                return;
            }

            CurrentWave = wave;
            Debug.Log($"[TimeWaveManager] Entered {CurrentWave} at {FormattedTime}");

            OnWaveChanged?.Invoke(CurrentWave);

            switch (CurrentWave)
            {
                case GameWave.Wave1:
                    onWave1Started?.Invoke();
                    break;
                case GameWave.Wave2:
                    onWave2Started?.Invoke();
                    break;
                case GameWave.Wave3:
                    onWave3Started?.Invoke();
                    break;
            }
        }

        private void CompleteShift()
        {
            HasShiftEnded = true;
            IsRunning = false;

            Debug.Log($"[TimeWaveManager] Shift Completed at {FormattedTime}");
            OnShiftEnded?.Invoke();
            onShiftEnded?.Invoke();
        }
    }
}
