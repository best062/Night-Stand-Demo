using UnityEngine;

namespace NightStand.Data
{
    /// <summary>
    /// ข้อมูลพื้นฐานของไอเทมและอุปกรณ์ (Item 1p)
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Night Stand/Data/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemName;
        [SerializeField] private ItemType itemType = ItemType.Flashlight;
        [SerializeField] private Sprite icon;

        [TextArea(2, 3)]
        [SerializeField] private string description;

        [Header("Prefabs")]
        [Tooltip("Prefab สำหรับถือในมือขวา (กรณีเป็น Equipment)")]
        [SerializeField] private GameObject equippedPrefab;

        [Tooltip("Prefab สำหรับวางในฉาก (เช่น บนโต๊ะป้อมยาม)")]
        [SerializeField] private GameObject worldPrefab;

        public string ItemName => itemName;
        public ItemType ItemType => itemType;
        public Sprite Icon => icon;
        public string Description => description;
        public GameObject EquippedPrefab => equippedPrefab;
        public GameObject WorldPrefab => worldPrefab;
    }
}
