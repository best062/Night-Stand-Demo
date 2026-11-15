using System.Collections;
using NightStand.Player;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// จัดการ Light Horror / Jumpscare Trigger (เสียงตกใจ + กล้องสั่น + Flash UI ชั่วคราว)
    /// </summary>
    public class JumpscareManager : MonoBehaviour
    {
        public static JumpscareManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private CanvasGroup horrorFlashCanvasGroup;

        [Header("Default Jumpscare Settings")]
        [SerializeField] private AudioClip defaultScareClip;
        [SerializeField] private float defaultShakeDuration = 0.4f;
        [SerializeField] private float defaultShakeIntensity = 0.08f;
        [SerializeField] private float flashFadeDuration = 0.35f;

        private Coroutine flashCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (cameraShake == null)
            {
                cameraShake = FindFirstObjectByType<CameraShake>();
            }

            if (horrorFlashCanvasGroup != null)
            {
                horrorFlashCanvasGroup.alpha = 0f;
                horrorFlashCanvasGroup.blocksRaycasts = false;
            }
        }

        public void TriggerJumpscare(AudioClip customClip = null, float? shakeDuration = null, float? shakeIntensity = null)
        {
            AudioClip clipToPlay = customClip != null ? customClip : defaultScareClip;
            if (audioSource != null && clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }

            if (cameraShake != null)
            {
                cameraShake.Shake(
                    shakeDuration ?? defaultShakeDuration,
                    shakeIntensity ?? defaultShakeIntensity
                );
            }

            if (horrorFlashCanvasGroup != null)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                }

                flashCoroutine = StartCoroutine(FlashVignetteRoutine());
            }
        }

        private IEnumerator FlashVignetteRoutine()
        {
            horrorFlashCanvasGroup.alpha = 0.75f;
            float elapsed = 0f;

            while (elapsed < flashFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                horrorFlashCanvasGroup.alpha = Mathf.Lerp(0.75f, 0f, Mathf.Clamp01(elapsed / flashFadeDuration));
                yield return null;
            }

            horrorFlashCanvasGroup.alpha = 0f;
            flashCoroutine = null;
        }
    }
}
