using UnityEngine;

namespace NightStand.Equipment
{
    /// <summary>
    /// ไฟฉาย: คลิกซ้ายเพื่อเปิด/ปิดไฟฉายส่องจุดอับในรถ
    /// </summary>
    public class FlashlightItem : EquipmentItemBase
    {
        [Header("Flashlight Components")]
        [SerializeField] private Light spotLight;
        [SerializeField] private bool startTurnedOn = false;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleClip;

        public bool IsOn { get; private set; }

        private void Awake()
        {
            if (spotLight == null)
            {
                spotLight = GetComponentInChildren<Light>(true);
            }

            SetLightState(startTurnedOn, playSound: false);
        }

        public override void OnUnequip()
        {
            SetLightState(false, playSound: false);
            base.OnUnequip();
        }

        public override bool TryUse()
        {
            SetLightState(!IsOn, playSound: true);
            return true;
        }

        private void SetLightState(bool on, bool playSound)
        {
            IsOn = on;

            if (spotLight != null)
            {
                spotLight.enabled = on;
            }

            if (playSound && audioSource != null && toggleClip != null)
            {
                audioSource.PlayOneShot(toggleClip);
            }
        }
    }
}
