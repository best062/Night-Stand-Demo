using NightStand.Core;
using UnityEngine;

namespace NightStand.Equipment
{
    /// <summary>
    /// กุญแจ: คลิกซ้ายเพื่อไขตู้เอกสารหรือเปิดท้ายรถในเคสพิเศษ (วัตถุที่มี ILockable)
    /// </summary>
    public class KeyItem : EquipmentItemBase
    {
        [Header("Key Settings")]
        [SerializeField] private string keyId;
        [SerializeField] private float useDistance = 3.0f;
        [SerializeField] private LayerMask targetLayerMask = ~0;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip useClip;
        [SerializeField] private AudioClip failClip;

        public override bool TryUse()
        {
            Transform cam = GetPlayerCamera();
            if (cam == null)
            {
                return true;
            }

            Ray ray = new Ray(cam.position, cam.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, useDistance, targetLayerMask, QueryTriggerInteraction.Collide))
            {
                ILockable lockable = hit.collider.GetComponentInParent<ILockable>();
                if (lockable != null && lockable.IsLocked)
                {
                    bool unlocked = lockable.TryUnlock(Manager != null ? Manager.gameObject : gameObject, keyId);
                    PlayClip(unlocked ? useClip : failClip);
                    return true;
                }
            }

            PlayClip(failClip);
            return true;
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
