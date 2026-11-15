using NightStand.Core;
using NightStand.Data;
using TMPro;
using UnityEngine;

namespace NightStand.UI
{
    /// <summary>
    /// แสดงผลนาฬิกาดิจิทัล (18:00 - 06:00) และ Wave ปัจจุบันบน UI
    /// </summary>
    public class DigitalClockUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TimeWaveManager timeWaveManager;
        [SerializeField] private TMP_Text clockText;
        [SerializeField] private TMP_Text waveText;

        private void Awake()
        {
            if (timeWaveManager == null)
            {
                timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
            }
        }

        private void OnEnable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnTimeUpdated += UpdateClockDisplay;
                timeWaveManager.OnWaveChanged += UpdateWaveDisplay;
            }
        }

        private void OnDisable()
        {
            if (timeWaveManager != null)
            {
                timeWaveManager.OnTimeUpdated -= UpdateClockDisplay;
                timeWaveManager.OnWaveChanged -= UpdateWaveDisplay;
            }
        }

        private void Start()
        {
            if (timeWaveManager != null)
            {
                UpdateClockDisplay(timeWaveManager.CurrentHour, timeWaveManager.CurrentMinute);
                UpdateWaveDisplay(timeWaveManager.CurrentWave);
            }
        }

        private void UpdateClockDisplay(int hour, int minute)
        {
            if (clockText != null)
            {
                clockText.text = $"{hour:00}:{minute:00}";
            }
        }

        private void UpdateWaveDisplay(GameWave wave)
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE {(int)wave}";
            }
        }
    }
}
