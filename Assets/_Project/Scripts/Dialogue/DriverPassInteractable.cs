using NightStand.Core;
using NightStand.Vehicle;
using UnityEngine;

namespace NightStand.Dialogue
{
    /// <summary>
    /// บัตรผ่านที่คนขับยื่นออกมาตรงหน้าต่างรถ กด E เพื่อรับบัตรไปวางบนโต๊ะป้อมยาม
    /// </summary>
    public class DriverPassInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private VehicleController vehicleController;

        public string InteractionPrompt
        {
            get
            {
                if (vehicleController != null && vehicleController.HasDriverHandedPass && !vehicleController.HasReturnedPassToDriver)
                {
                    return "Return Pass to Driver";
                }

                return "Take Entry Pass";
            }
        }

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

            if (!vehicleController.HasDriverHandedPass)
            {
                vehicleController.TakePassFromDriver();
            }
            else if (!vehicleController.HasReturnedPassToDriver)
            {
                vehicleController.ReturnPassToDriver();
            }
        }
    }
}
