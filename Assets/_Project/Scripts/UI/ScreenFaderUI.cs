using System.Collections;
using UnityEngine;

namespace NightStand.UI
{
    /// <summary>
    /// ควบคุมการตัดภาพเข้าฉากดำ (Fade to Black) และตัดภาพกลับมา (Fade In)
    /// </summary>
    public class ScreenFaderUI : MonoBehaviour
    {
        public static ScreenFaderUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        private Coroutine currentFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }
        }

        public Coroutine FadeOut(float duration)
        {
            return StartFade(1f, duration, blockRaycasts: true);
        }

        public Coroutine FadeIn(float duration)
        {
            return StartFade(0f, duration, blockRaycasts: false);
        }

        public void SetBlackImmediate(bool isBlack)
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = isBlack ? 1f : 0f;
                fadeCanvasGroup.blocksRaycasts = isBlack;
            }
        }

        private Coroutine StartFade(float targetAlpha, float duration, bool blockRaycasts)
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }

            currentFadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, blockRaycasts));
            return currentFadeCoroutine;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool blockRaycasts)
        {
            if (fadeCanvasGroup == null)
            {
                yield break;
            }

            fadeCanvasGroup.blocksRaycasts = blockRaycasts;
            float startAlpha = fadeCanvasGroup.alpha;

            if (duration <= 0f)
            {
                fadeCanvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
            currentFadeCoroutine = null;
        }
    }
}
