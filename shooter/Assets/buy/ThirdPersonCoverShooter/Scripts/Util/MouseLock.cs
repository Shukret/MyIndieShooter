using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Locks mouse cursor inside the game window. 
    /// Locked by pressing left mouse button, unlocked by pressing the escape key.
    /// </summary>
    public class MouseLock : MonoBehaviour
    {
        [SerializeField] private Inventory playerInventory;
        private bool _isLocked = true;

        private void LateUpdate()
        {
            if (playerInventory.active)
                _isLocked = false;
            if (Input.GetKeyDown(KeyCode.Escape))
                _isLocked = false;

            if (Input.GetMouseButtonDown(0) && !playerInventory.active)
                _isLocked = true;
            if (!playerInventory.active && Input.GetKeyDown(KeyCode.I))
                _isLocked = true;
            if (_isLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}