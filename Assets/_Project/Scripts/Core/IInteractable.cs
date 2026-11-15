using UnityEngine;

namespace NightStand.Core
{
    /// <summary>
    /// Interface สำหรับวัตถุที่ผู้เล่นสามารถเล็ง Crosshair และกด E / คลิกซ้าย เพื่อโต้ตอบได้
    /// </summary>
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameObject interactor);
    }
}
