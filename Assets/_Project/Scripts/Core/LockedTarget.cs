using UnityEngine;
using UnityEngine.Events;

namespace NightStand.Core
{
    /// <summary>
    /// คอมโพเนนต์สำหรับวัตถุที่ล็อกอยู่ (ตู้เอกสารในป้อมยาม หรือท้ายรถ)
    /// กด E เพื่อตรวจสอบสถานะ และใช้ Key คลิกซ้ายเพื่อปลดล็อก
    /// </summary>
    public class LockedTarget : MonoBehaviour, IInteractable, ILockable
    {
        [Header("Lock Settings")]
        [SerializeField] private bool startLocked = true;

        [Tooltip("ปล่อยว่างไว้เพื่อให้ Key ทั่วไปไขได้ หรือระบุรหัสกุญแจเฉพาะ")]
        [SerializeField] private string requiredKeyId;

        [SerializeField] private string lockedPrompt = "Locked (Use Key)";
        [SerializeField] private string unlockedPrompt = "Open / Inspect";

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip lockedClip;
        [SerializeField] private AudioClip unlockClip;

        [Header("Events")]
        [SerializeField] private UnityEvent onUnlocked;
        [SerializeField] private UnityEvent onInteractWhenUnlocked;

        public bool IsLocked { get; private set; }

        public string InteractionPrompt => IsLocked ? lockedPrompt : unlockedPrompt;

        private void Awake()
        {
            IsLocked = startLocked;
        }

        public void Interact(GameObject interactor)
        {
            if (IsLocked)
            {
                PlayClip(lockedClip);
                Debug.Log($"[LockedTarget] {name} is locked. Equip Key and Left-Click to unlock.");
                return;
            }

            onInteractWhenUnlocked?.Invoke();
        }

        public bool TryUnlock(GameObject user, string keyId = null)
        {
            if (!IsLocked)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(requiredKeyId) && requiredKeyId != keyId)
            {
                PlayClip(lockedClip);
                Debug.Log($"[LockedTarget] Wrong key for {name}.");
                return false;
            }

            IsLocked = false;
            PlayClip(unlockClip);
            Debug.Log($"[LockedTarget] {name} unlocked!");
            onUnlocked?.Invoke();
            return true;
        }

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
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
