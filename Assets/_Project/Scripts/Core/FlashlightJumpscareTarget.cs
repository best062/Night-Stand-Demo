using NightStand.Data;
using NightStand.Equipment;
using NightStand.Vehicle;
using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// ติดบนจุดซ่อนตัวของ Anomaly (เช่น เบาะหลัง Stowaway) เมื่อผู้เล่นส่องไฟฉายโดนใน Wave 2-3 จะทริกเกอร์ Jumpscare เบาๆ
    /// </summary>
    public class FlashlightJumpscareTarget : MonoBehaviour
    {
        [Header("Condition")]
        [SerializeField] private GameWave minWave = GameWave.Wave2;
        [SerializeField] private bool onlyWhenAnomalyVehicle = true;
        [SerializeField] private float triggerDistance = 4.5f;
        [SerializeField, Range(5f, 60f)] private float beamHalfAngle = 22f;

        [Header("Jumpscare")]
        [SerializeField] private AudioClip scareClip;
        [SerializeField] private float shakeDuration = 0.45f;
        [SerializeField] private float shakeIntensity = 0.08f;

        private VehicleController vehicleController;
        private TimeWaveManager timeWaveManager;
        private FlashlightItem flashlight;
        private bool hasTriggered;

        private void Awake()
        {
            vehicleController = GetComponentInParent<VehicleController>();
            timeWaveManager = FindFirstObjectByType<TimeWaveManager>();
        }

        private void Update()
        {
            if (hasTriggered || JumpscareManager.Instance == null)
            {
                return;
            }

            if (vehicleController != null)
            {
                if (vehicleController.CurrentState != VehicleState.WaitingAtGate)
                {
                    return;
                }

                if (onlyWhenAnomalyVehicle && (vehicleController.VehicleData == null || !vehicleController.VehicleData.IsAnomalyVehicle))
                {
                    return;
                }
            }

            if (timeWaveManager != null && (int)timeWaveManager.CurrentWave < (int)minWave)
            {
                return;
            }

            if (flashlight == null)
            {
                flashlight = FindFirstObjectByType<FlashlightItem>();
                if (flashlight == null)
                {
                    return;
                }
            }

            if (!flashlight.gameObject.activeInHierarchy || !flashlight.IsOn)
            {
                return;
            }

            Transform lightTransform = flashlight.transform;
            Vector3 toTarget = transform.position - lightTransform.position;
            float distance = toTarget.magnitude;

            if (distance > triggerDistance)
            {
                return;
            }

            float angle = Vector3.Angle(lightTransform.forward, toTarget.normalized);
            if (angle <= beamHalfAngle)
            {
                hasTriggered = true;
                JumpscareManager.Instance.TriggerJumpscare(scareClip, shakeDuration, shakeIntensity);
            }
        }
    }
}
