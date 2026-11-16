using NightStand.Core;
using NightStand.Data;
using NightStand.Dialogue;
using NightStand.Player;
using NightStand.Vehicle;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NightStand.UI
{
    /// <summary>
    /// หน้าต่าง Dialogue with NPC (3p) แบบมีตัวเลือกคำถามเพื่อจับผิดคำพูด
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text npcNameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Question Buttons")]
        [SerializeField] private Button askIdentityButton;
        [SerializeField] private Button askPurposeButton;
        [SerializeField] private Button askSuspiciousButton;
        [SerializeField] private Button closeButton;

        [Header("Audio & Player (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private FirstPersonController playerController;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private NPCDataSO currentNpc;
        private NPCDialogueInteractable currentInteractable;

        // [Data Structure: N-ary Tree (DialogueTree) - Branching Dialogue State]
        // Tracks the root tree, current node, and current interrogation depth for the active NPC.
        // Time Complexity: Node Transition O(1) per branch selection
        private DialogueTree dialogueTree;
        private DialogueNode currentNode;
        private DialogueNode suspiciousNode;

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

            if (askIdentityButton != null) askIdentityButton.onClick.AddListener(OnAskIdentity);
            if (askPurposeButton != null) askPurposeButton.onClick.AddListener(OnAskPurpose);
            if (askSuspiciousButton != null) askSuspiciousButton.onClick.AddListener(OnAskSuspicious);
            if (closeButton != null) closeButton.onClick.AddListener(CloseDialogue);

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (askIdentityButton != null) askIdentityButton.onClick.RemoveListener(OnAskIdentity);
            if (askPurposeButton != null) askPurposeButton.onClick.RemoveListener(OnAskPurpose);
            if (askSuspiciousButton != null) askSuspiciousButton.onClick.RemoveListener(OnAskSuspicious);
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseDialogue);
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseDialogue();
            }
        }

        public void StartDialogue(VehicleController vehicle, NPCDialogueInteractable interactable)
        {
            if (vehicle == null || vehicle.VehicleData == null || vehicle.VehicleData.NpcData == null)
            {
                return;
            }

            currentNpc = vehicle.VehicleData.NpcData;
            currentInteractable = interactable;
            dialogueTree = DialogueTree.BuildFromNPC(currentNpc);
            currentNode = dialogueTree.Root;
            suspiciousNode = null;

            if (npcNameText != null)
            {
                npcNameText.text = string.IsNullOrWhiteSpace(currentNpc.NpcName) ? "Driver" : currentNpc.NpcName;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = currentNpc.Portrait;
                portraitImage.gameObject.SetActive(currentNpc.Portrait != null);
            }

            SetDialogueLine(currentNode != null ? currentNode.ResponseLine : currentNpc.GetGreeting());

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(false);
            }
        }

        public void CloseDialogue()
        {
            currentNpc = null;
            currentInteractable = null;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (playerController != null)
            {
                playerController.SetGameplayInputEnabled(true);
            }
        }

        // [Operation: Tree Branch Traversal O(1) - Child 0: Identity]
        private void OnAskIdentity()
        {
            if (dialogueTree == null || dialogueTree.Root == null)
            {
                return;
            }

            currentNode = dialogueTree.Root.GetChild(0);
            if (currentNode != null)
            {
                SetDialogueLine(currentNode.ResponseLine);
            }
        }

        // [Operation: Tree Branch Traversal O(1) - Child 1: Purpose]
        private void OnAskPurpose()
        {
            if (dialogueTree == null || dialogueTree.Root == null)
            {
                return;
            }

            currentNode = dialogueTree.Root.GetChild(1);
            if (currentNode != null)
            {
                SetDialogueLine(currentNode.ResponseLine);
            }
        }

        // [Operation: Subtree Depth Traversal O(1) per click - Child 2 -> Escalation Subtree]
        private void OnAskSuspicious()
        {
            if (currentNpc == null || currentInteractable == null || dialogueTree == null || dialogueTree.Root == null)
            {
                return;
            }

            int pressCount = currentInteractable.IncrementSuspiciousPress();

            if (suspiciousNode == null)
            {
                suspiciousNode = dialogueTree.Root.GetChild(2);
            }
            else if (suspiciousNode.Children.Count > 0)
            {
                suspiciousNode = suspiciousNode.GetChild(0);
            }

            currentNode = suspiciousNode;
            if (currentNode != null)
            {
                SetDialogueLine(currentNode.ResponseLine);
            }

            if (pressCount >= 2 && currentNpc.SuspiciousJumpscareClip != null)
            {
                if (JumpscareManager.Instance != null)
                {
                    JumpscareManager.Instance.TriggerJumpscare(currentNpc.SuspiciousJumpscareClip);
                }
                else if (audioSource != null)
                {
                    audioSource.PlayOneShot(currentNpc.SuspiciousJumpscareClip);
                }
            }
        }

        private void SetDialogueLine(string line)
        {
            if (dialogueText != null)
            {
                dialogueText.text = line;
            }
        }
    }
}
