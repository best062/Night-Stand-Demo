using System.Collections;
using NightStand.Core;
using NightStand.Data;
using NightStand.Player;
using NightStand.UI;
using NightStand.Vehicle;
using UnityEngine;

namespace NightStand.Equipment
{
    /// <summary>
    /// อุปกรณ์ปราบ Anomaly: เล็ง Crosshair ไปที่รถแล้วคลิกซ้าย -> เล่น VFX/SFX -> Fade to Black -> เสียงกำจัด -> ลบรถ -> ตัดภาพกลับ
    /// หากใช้กับคนปกติ จะส่งสัญญาณไปยัง GameManager เพื่อทริกเกอร์ฉากจบถูกจับกุมทันที
    /// </summary>
    public class EliminatorItem : EquipmentItemBase
    {
        [Header("Targeting")]
        [SerializeField] private float useDistance = 8.0f;
        [SerializeField] private LayerMask targetLayerMask = ~0;

        [Header("Effects & Audio")]
        [SerializeField] private ParticleSystem fireParticle;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fireClip;
        [SerializeField] private AudioClip eliminateClip;

        [Header("Fade Timing")]
        [SerializeField] private float preFadeDelay = 0.45f;
        [SerializeField] private float fadeOutDuration = 0.35f;
        [SerializeField] private float blackHoldDuration = 0.85f;
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private bool fadeInOnGameOver = true;

        private bool isEliminating;

        public override bool TryUse()
        {
            if (isEliminating)
            {
                return true;
            }

            Transform cam = GetPlayerCamera();
            if (cam == null)
            {
                return true;
            }

            Ray ray = new Ray(cam.position, cam.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, useDistance, targetLayerMask, QueryTriggerInteraction.Collide))
            {
                VehicleController vehicle = hit.collider.GetComponentInParent<VehicleController>();
                if (vehicle != null && vehicle.CurrentState != VehicleState.Exited)
                {
                    StartCoroutine(EliminateSequence(vehicle));
                }
            }

            return true;
        }

        private IEnumerator EliminateSequence(VehicleController targetVehicle)
        {
            isEliminating = true;

            FirstPersonController fps = Manager != null ? Manager.GetComponentInParent<FirstPersonController>() : null;
            if (fps != null)
            {
                fps.SetGameplayInputEnabled(false);
            }

            if (fireParticle != null)
            {
                fireParticle.Play();
            }

            if (audioSource != null && fireClip != null)
            {
                audioSource.PlayOneShot(fireClip);
            }

            yield return new WaitForSeconds(preFadeDelay);

            if (ScreenFaderUI.Instance != null)
            {
                yield return ScreenFaderUI.Instance.FadeOut(fadeOutDuration);
            }

            if (audioSource != null && eliminateClip != null)
            {
                audioSource.PlayOneShot(eliminateClip);
            }

            yield return new WaitForSeconds(blackHoldDuration);

            if (targetVehicle != null)
            {
                if (GameManager.Instance != null && targetVehicle.VehicleData != null)
                {
                    GameManager.Instance.RegisterVehicleDecision(targetVehicle.VehicleData, PlayerDecision.Eliminate);
                }

                targetVehicle.EliminateAndRemove();
            }

            bool isGameOver = GameManager.Instance != null && GameManager.Instance.IsGameOver;

            if (!isGameOver || fadeInOnGameOver)
            {
                if (ScreenFaderUI.Instance != null)
                {
                    yield return ScreenFaderUI.Instance.FadeIn(fadeInDuration);
                }
            }

            if (fps != null)
            {
                if (isGameOver)
                {
                    fps.SetCursorLocked(false);
                }
                else
                {
                    fps.SetGameplayInputEnabled(true);
                }
            }

            isEliminating = false;
        }
    }
}
