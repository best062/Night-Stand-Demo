using NightStand.Data;
using UnityEngine;

namespace NightStand.Equipment
{
    /// <summary>
    /// คลาสแม่สำหรับอุปกรณ์ที่ผู้เล่นถือในมือขวา (Flashlight, Key, Eliminator)
    /// </summary>
    public abstract class EquipmentItemBase : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemDataSO itemData;

        public ItemDataSO ItemData => itemData;
        public string DisplayName => itemData != null && !string.IsNullOrWhiteSpace(itemData.ItemName)
            ? itemData.ItemName
            : gameObject.name;

        protected EquipmentManager Manager { get; private set; }

        public virtual void OnEquip(EquipmentManager manager)
        {
            Manager = manager;
            gameObject.SetActive(true);
        }

        public virtual void OnUnequip()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// เรียกเมื่อผู้เล่นกดคลิกซ้ายขณะถืออุปกรณ์ชิ้นนี้
        /// คืนค่า true หากอุปกรณ์รับอินพุตคลิกซ้ายไปแล้ว
        /// </summary>
        public abstract bool TryUse();

        protected Transform GetPlayerCamera()
        {
            return Manager != null ? Manager.PlayerCamera : null;
        }
    }
}
