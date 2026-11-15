using NightStand.Core;
using NightStand.Equipment;
using UnityEngine;

namespace NightStand.Items
{
    /// <summary>
    /// ไอเทมที่เพื่อน AI ส่งมาวางบนโต๊ะป้อมยาม (เช่น Key หรือ Eliminator)
    /// ผู้เล่นกด E เพื่อหยิบและปลดล็อกช่องอุปกรณ์ใน EquipmentManager
    /// </summary>
    public class EquipmentPickupInteractable : MonoBehaviour, IInteractable
    {
        [Header("Pickup Settings")]
        [SerializeField] private string itemDisplayName = "Equipment";
        [SerializeField] private int equipmentSlotIndex = 2;
        [SerializeField] private bool equipImmediately = true;
        [SerializeField] private bool destroyOnPickup = true;

        public string InteractionPrompt => $"Pick up {itemDisplayName}";

        public void Interact(GameObject interactor)
        {
            EquipmentManager eqManager = interactor != null
                ? interactor.GetComponentInChildren<EquipmentManager>()
                : FindFirstObjectByType<EquipmentManager>();

            if (eqManager != null)
            {
                eqManager.UnlockSlot(equipmentSlotIndex, equipImmediately);
            }
            else
            {
                Debug.LogWarning("[EquipmentPickupInteractable] EquipmentManager not found.");
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
