using NightStand.Data;
using NightStand.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NightStand.UI
{
    /// <summary>
    /// หน้าต่าง 2D UI Overlay สำหรับเปิดดูเอกสาร (รายชื่อลูกบ้าน / บัตรผ่าน) พร้อมปุ่มปิด
    /// </summary>
    public class DocumentViewerUI : MonoBehaviour
    {
        public static DocumentViewerUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image documentImage;
        [SerializeField] private Button closeButton;

        [Header("Player Reference (Auto-find if empty)")]
        [SerializeField] private FirstPersonController playerController;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null &&
                (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
            {
                Close();
            }
        }

        public void OpenDocument(DocumentDataSO data)
        {
            if (data == null)
            {
                return;
            }

            OpenCustomDocument(data.DocumentTitle, data.GetFormattedContent(), data.DocumentSprite);
        }

        public void OpenCustomDocument(string title, string content, Sprite sprite = null)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = content;
            }

            if (documentImage != null)
            {
                documentImage.sprite = sprite;
                documentImage.gameObject.SetActive(sprite != null);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(false);
            }
        }

        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(true);
            }
        }
    }
}
