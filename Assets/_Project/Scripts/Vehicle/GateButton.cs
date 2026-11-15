using NightStand.Core;
using UnityEngine;

namespace NightStand.Vehicle
{
    public enum GateButtonType
    {
        Allow,
        Deny
    }

    /// <summary>
    /// ปุ่มกดหน้าป้อมยาม (Allow / Deny) สำหรับโต้ตอบผ่าน Raycast Interact
    /// </summary>
    public class GateButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private GateButtonType buttonType = GateButtonType.Allow;
        [SerializeField] private GateController gateController;

        public string InteractionPrompt => buttonType == GateButtonType.Allow
            ? "Allow Vehicle (Open Gate)"
            : "Deny Vehicle (Turn Back)";

        private void Awake()
        {
            if (gateController == null)
            {
                gateController = FindFirstObjectByType<GateController>();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (gateController == null)
            {
                Debug.LogWarning("[GateButton] GateController reference is missing.");
                return;
            }

            if (buttonType == GateButtonType.Allow)
            {
                gateController.AllowCurrentVehicle();
            }
            else
            {
                gateController.DenyCurrentVehicle();
            }
        }
    }
}
