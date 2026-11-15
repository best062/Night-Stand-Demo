using System;
using NightStand.Core;
using NightStand.Equipment;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightStand.Player
{
    /// <summary>
    /// ควบคุมตัวละครมุมมองบุคคลที่หนึ่ง (WASD, Mouse Look) และระบบ Raycast Interact (E หรือ Left Click)
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 6.5f;
        [SerializeField] private float gravity = -19.62f;

        [Header("Mouse Look")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float maxLookAngle = 85f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 3.0f;
        [SerializeField] private LayerMask interactLayerMask = ~0;

        [Header("Equipment (Optional)")]
        [SerializeField] private EquipmentManager equipmentManager;

        public event Action<IInteractable> OnHoverInteractableChanged;

        public IInteractable CurrentInteractable { get; private set; }
        public bool IsCursorLocked { get; private set; }
        public bool IsGameplayInputEnabled { get; private set; } = true;
        public Transform PlayerCamera => playerCamera;
        public LayerMask InteractLayerMask => interactLayerMask;

        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraPitch;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    playerCamera = cam.transform;
                }
            }

            if (equipmentManager == null)
            {
                equipmentManager = GetComponentInChildren<EquipmentManager>();
            }
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (!IsGameplayInputEnabled)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(!IsCursorLocked);
            }

            if (IsCursorLocked)
            {
                HandleMouseLook();
            }

            HandleMovement();
            HandleInteractionRaycast();
            HandleInteractionInput();
        }

        public void SetGameplayInputEnabled(bool enabled)
        {
            IsGameplayInputEnabled = enabled;
            SetCursorLocked(enabled);

            if (equipmentManager != null)
            {
                equipmentManager.SetEquipmentInputEnabled(enabled);
            }

            if (!enabled && CurrentInteractable != null)
            {
                CurrentInteractable = null;
                OnHoverInteractableChanged?.Invoke(null);
            }
        }

        public void SetCursorLocked(bool locked)
        {
            IsCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void HandleMouseLook()
        {
            if (playerCamera == null || Mouse.current == null)
            {
                return;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            cameraPitch = Mathf.Clamp(cameraPitch - mouseDelta.y, -maxLookAngle, maxLookAngle);
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            transform.Rotate(Vector3.up * mouseDelta.x);
        }

        private void HandleMovement()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.wKey.isPressed) vertical += 1f;

            Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
            float currentSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = moveDirection * currentSpeed + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void HandleInteractionRaycast()
        {
            if (playerCamera == null)
            {
                return;
            }

            IInteractable hitInteractable = null;
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
            {
                hitInteractable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (!ReferenceEquals(hitInteractable, CurrentInteractable))
            {
                CurrentInteractable = hitInteractable;
                OnHoverInteractableChanged?.Invoke(CurrentInteractable);
            }
        }

        private void HandleInteractionInput()
        {
            bool pressedInteract = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            bool clickedLeft = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && IsCursorLocked;

            if (pressedInteract && CurrentInteractable != null)
            {
                CurrentInteractable.Interact(gameObject);
                return;
            }

            if (clickedLeft)
            {
                // หากถืออุปกรณ์ในมือขวา ให้ใช้คลิกซ้ายใช้งานอุปกรณ์ก่อน (เช่น ไฟฉาย, กุญแจ, Eliminator)
                if (equipmentManager != null && equipmentManager.TryUseCurrentItem())
                {
                    return;
                }

                if (CurrentInteractable != null)
                {
                    CurrentInteractable.Interact(gameObject);
                }
            }
        }
    }
}
