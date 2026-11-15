using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightStand.Equipment
{
    [Serializable]
    public class EquipmentSlot
    {
        public string slotLabel = "Slot";
        public EquipmentItemBase itemInstance;
        public bool unlocked = true;
    }

    /// <summary>
    /// ระบบ Equipment (2p): ถือของมือขวาได้ทีละ 1 ชิ้น สลับช่องด้วยปุ่ม 1, 2, 3 หรือ Scroll Wheel
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Hand Anchor & Camera")]
        [SerializeField] private Transform rightHandAnchor;
        [SerializeField] private Transform playerCamera;

        [Header("3 Equipment Slots (1 = Flashlight, 2 = Key, 3 = Eliminator)")]
        [SerializeField] private List<EquipmentSlot> slots = new List<EquipmentSlot>(3);
        [SerializeField] private int startingSlotIndex = 0;

        public event Action<int, EquipmentItemBase> OnEquipmentChanged;

        public Transform PlayerCamera => playerCamera;
        public int CurrentSlotIndex { get; private set; } = -1;
        public EquipmentItemBase CurrentItem { get; private set; }
        public bool HasEquippedItem => CurrentItem != null;
        public bool IsInputEnabled { get; private set; } = true;
        public IReadOnlyList<EquipmentSlot> Slots => slots;

        private void Awake()
        {
            if (playerCamera == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    playerCamera = cam.transform;
                }
            }

            DeactivateAllItems();
        }

        private void Start()
        {
            if (slots.Count > 0 && startingSlotIndex >= 0 && startingSlotIndex < slots.Count)
            {
                EquipSlot(startingSlotIndex);
            }
            else
            {
                OnEquipmentChanged?.Invoke(-1, null);
            }
        }

        private void Update()
        {
            if (!IsInputEnabled || slots.Count == 0)
            {
                return;
            }

            HandleSlotSwitchInput();
        }

        public void SetEquipmentInputEnabled(bool enabled)
        {
            IsInputEnabled = enabled;
        }

        public bool TryUseCurrentItem()
        {
            if (!IsInputEnabled || CurrentItem == null)
            {
                return false;
            }

            return CurrentItem.TryUse();
        }

        public void EquipSlot(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                return;
            }

            EquipmentSlot targetSlot = slots[index];
            if (!targetSlot.unlocked || targetSlot.itemInstance == null)
            {
                return;
            }

            if (CurrentSlotIndex == index && CurrentItem == targetSlot.itemInstance)
            {
                return;
            }

            if (CurrentItem != null)
            {
                CurrentItem.OnUnequip();
            }

            CurrentSlotIndex = index;
            CurrentItem = targetSlot.itemInstance;
            CurrentItem.OnEquip(this);

            OnEquipmentChanged?.Invoke(CurrentSlotIndex, CurrentItem);
        }

        public void UnequipCurrent()
        {
            if (CurrentItem != null)
            {
                CurrentItem.OnUnequip();
            }

            CurrentSlotIndex = -1;
            CurrentItem = null;
            OnEquipmentChanged?.Invoke(-1, null);
        }

        /// <summary>
        /// ปลดล็อกหรือเพิ่มอุปกรณ์ใหม่เข้าสู่ Slot (รองรับระบบ Quest / AI Partner ใน Phase 4)
        /// </summary>
        public void UnlockSlot(int index, bool equipImmediately = false)
        {
            if (index < 0 || index >= slots.Count)
            {
                return;
            }

            slots[index].unlocked = true;
            if (equipImmediately)
            {
                EquipSlot(index);
            }
        }

        private void HandleSlotSwitchInput()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) EquipSlot(0);
                else if (Keyboard.current.digit2Key.wasPressedThisFrame) EquipSlot(1);
                else if (Keyboard.current.digit3Key.wasPressedThisFrame) EquipSlot(2);
            }

            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scrollY) > 0.01f)
                {
                    CycleSlot(scrollY > 0f ? -1 : 1);
                }
            }
        }

        private void CycleSlot(int direction)
        {
            if (slots.Count == 0)
            {
                return;
            }

            int start = CurrentSlotIndex < 0 ? 0 : CurrentSlotIndex;
            for (int step = 1; step <= slots.Count; step++)
            {
                int candidate = (start + direction * step + slots.Count) % slots.Count;
                if (slots[candidate].unlocked && slots[candidate].itemInstance != null)
                {
                    EquipSlot(candidate);
                    return;
                }
            }
        }

        private void DeactivateAllItems()
        {
            foreach (EquipmentSlot slot in slots)
            {
                if (slot != null && slot.itemInstance != null)
                {
                    if (rightHandAnchor != null && slot.itemInstance.transform.parent != rightHandAnchor)
                    {
                        slot.itemInstance.transform.SetParent(rightHandAnchor, false);
                    }

                    slot.itemInstance.gameObject.SetActive(false);
                }
            }
        }
    }
}
