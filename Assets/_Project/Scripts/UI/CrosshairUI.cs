using NightStand.Core;
using NightStand.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightStand.UI
{
    /// <summary>
    /// แสดงจุด Crosshair กลางจอและข้อความแจ้งเตือนเมื่อเล็งไปยังวัตถุที่ Interact ได้
    /// </summary>
    public class CrosshairUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private Image crosshairImage;
        [SerializeField] private TMP_Text interactionPromptText;

        [Header("Appearance")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color interactableColor = Color.yellow;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
            }
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.OnHoverInteractableChanged += HandleHoverChanged;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.OnHoverInteractableChanged -= HandleHoverChanged;
            }
        }

        private void Start()
        {
            HandleHoverChanged(null);
        }

        private void HandleHoverChanged(IInteractable interactable)
        {
            bool hasTarget = interactable != null;

            if (crosshairImage != null)
            {
                crosshairImage.color = hasTarget ? interactableColor : normalColor;
            }

            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(hasTarget);
                if (hasTarget)
                {
                    interactionPromptText.text = $"[E / Click] {interactable.InteractionPrompt}";
                }
            }
        }
    }
}
