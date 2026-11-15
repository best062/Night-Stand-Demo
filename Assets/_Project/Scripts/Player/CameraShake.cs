using System.Collections;
using UnityEngine;

namespace NightStand.Player
{
    /// <summary>
    /// เอฟเฟกต์กล้องสั่นแบบ localPosition เพื่อไม่ขัดแย้งกับการหมุนมุมกล้องของ FirstPersonController
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private Vector3 originalLocalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            originalLocalPosition = transform.localPosition;
        }

        public void Shake(float duration, float intensity)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalLocalPosition;
            }

            shakeCoroutine = StartCoroutine(ShakeRoutine(duration, intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float intensity)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                Vector3 offset = Random.insideUnitSphere * (intensity * damper);
                offset.z = 0f;
                transform.localPosition = originalLocalPosition + offset;
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            shakeCoroutine = null;
        }
    }
}
