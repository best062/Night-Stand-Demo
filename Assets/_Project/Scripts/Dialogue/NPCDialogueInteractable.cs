using NightStand.Core;
using NightStand.UI;
using NightStand.Vehicle;
using UnityEngine;

namespace NightStand.Dialogue
{
    /// <summary>
    /// จุดโต้ตอบสำหรับพูดคุยซักถาม NPC (ติดบนหน้าต่างคนขับของ Vehicle Prefab)
    /// </summary>
    public class NPCDialogueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private string promptText = "Talk to Driver";

        public int SuspiciousPressCount { get; private set; }

        public string InteractionPrompt => promptText;

        private void Awake()
        {
            if (vehicleController == null)
            {
                vehicleController = GetComponentInParent<VehicleController>();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (vehicleController == null || vehicleController.CurrentState != VehicleState.WaitingAtGate)
            {
                return;
            }

            if (vehicleController.VehicleData == null || vehicleController.VehicleData.NpcData == null)
            {
                Debug.LogWarning("[NPCDialogueInteractable] VehicleDataSO or NPCDataSO is missing.");
                return;
            }

            if (DialogueUI.Instance == null)
            {
                Debug.LogWarning("[NPCDialogueInteractable] DialogueUI.Instance is missing in scene.");
                return;
            }

            DialogueUI.Instance.StartDialogue(vehicleController, this);
        }

        public int IncrementSuspiciousPress()
        {
            SuspiciousPressCount++;
            return SuspiciousPressCount;
        }
    }
}
