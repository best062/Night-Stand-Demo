using NightStand.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightStand.UI
{
    /// <summary>
    /// แสดงชื่ออุปกรณ์ที่ถืออยู่และไฮไลต์ช่อง 1-2-3 บน HUD
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private TMP_Text activeItemNameText;
        [SerializeField] private Image[] slotBackgrounds;

        [Header("Colors")]
        [SerializeField] private Color normalSlotColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        [SerializeField] private Color selectedSlotColor = new Color(0.95f, 0.75f, 0.2f, 0.9f);

        private void Awake()
        {
            if (equipmentManager == null)
            {
                equipmentManager = FindFirstObjectByType<EquipmentManager>();
            }
        }

        private void OnEnable()
        {
            if (equipmentManager != null)
            {
                equipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            }
        }

        private void OnDisable()
        {
            if (equipmentManager != null)
            {
                equipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            }
        }

        private void HandleEquipmentChanged(int slotIndex, EquipmentItemBase item)
        {
            if (activeItemNameText != null)
            {
                activeItemNameText.text = item != null ? item.DisplayName : "Empty Hand";
            }

            if (slotBackgrounds != null)
            {
                for (int i = 0; i < slotBackgrounds.Length; i++)
                {
                    if (slotBackgrounds[i] != null)
                    {
                        slotBackgrounds[i].color = i == slotIndex ? selectedSlotColor : normalSlotColor;
                    }
                }
            }
        }
    }
}
