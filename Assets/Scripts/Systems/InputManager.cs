using UnityEngine;

namespace ZONE.Systems
{
    /// <summary>
    /// Central input manager. Handles all player input through a single point,
    /// making it easy to remap controls and support gamepad.
    /// Uses Unity's legacy Input Manager (works without Input System package).
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private bool useGamepad = false;
        [SerializeField] private float gamepadLookSensitivity = 2.5f;

        // Action callbacks
        public System.Action OnJump;
        public System.Action OnInteract;
        public System.Action OnAltInteract;
        public System.Action OnReload;
        public System.Action OnFire;
        public System.Action OnAim;
        public System.Action OnFlashlight;
        public System.Action OnInventoryToggle;
        public System.Action OnMapToggle;
        public System.Action OnCrouchToggle;
        public System.Action OnQuickSave;

        // Axis values (read each frame)
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float ScrollInput { get; private set; }

        // Quick slot selection (returns -1 if none pressed)
        public int QuickSlotInput { get; private set; }

        [Header("Key Bindings (Editable)")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode altInteractKey = KeyCode.F;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private KeyCode inventoryKey = KeyCode.Tab;
        [SerializeField] private KeyCode mapKey = KeyCode.M;
        [SerializeField] private KeyCode flashlightKey = KeyCode.L;
        [SerializeField] private KeyCode quickSaveKey = KeyCode.F5;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode[] quickSlotKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

        private void Update()
        {
            ReadAxes();
            ReadActions();
            ReadQuickSlots();
        }

        private void ReadAxes()
        {
            // Movement (WASD or left stick)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(moveX, moveZ).normalized;

            // Look (mouse or right stick)
            if (!useGamepad)
            {
                float lookX = Input.GetAxisRaw("Mouse X");
                float lookY = Input.GetAxisRaw("Mouse Y");
                LookInput = new Vector2(lookX, lookY);
            }
            else
            {
                float lookX = Input.GetAxis("RightStickHorizontal");
                float lookY = Input.GetAxis("RightStickVertical");
                LookInput = new Vector2(lookX, lookY) * gamepadLookSensitivity;
            }

            // Scroll (for weapon switching)
            ScrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        }

        private void ReadActions()
        {
            // Continuous inputs (held down)
            // Fire and aim are continuous — checked by weapon system directly via Input.GetKey

            // Discrete inputs (pressed this frame)
            if (Input.GetKeyDown(KeyCode.Space)) OnJump?.Invoke();
            if (Input.GetKeyDown(interactKey)) OnInteract?.Invoke();
            if (Input.GetKeyDown(altInteractKey)) OnAltInteract?.Invoke();
            if (Input.GetKeyDown(reloadKey)) OnReload?.Invoke();
            if (Input.GetKeyDown(inventoryKey)) OnInventoryToggle?.Invoke();
            if (Input.GetKeyDown(mapKey)) OnMapToggle?.Invoke();
            if (Input.GetKeyDown(flashlightKey)) OnFlashlight?.Invoke();
            if (Input.GetKeyDown(quickSaveKey)) OnQuickSave?.Invoke();

            // Crouch is a held key (not toggle by default)
            // The PlayerController handles this directly
        }

        private void ReadQuickSlots()
        {
            QuickSlotInput = -1;

            for (int i = 0; i < quickSlotKeys.Length; i++)
            {
                if (Input.GetKeyDown(quickSlotKeys[i]))
                {
                    QuickSlotInput = i;
                    return;
                }
            }

            // Also support scroll wheel for weapon switching
            if (ScrollInput > 0f) QuickSlotInput = -2; // Scroll up = previous
            else if (ScrollInput < 0f) QuickSlotInput = -3; // Scroll down = next
        }

        /// <summary>
        /// Check if the fire button is currently held.
        /// </summary>
        public bool IsFiring()
        {
            return Input.GetButton("Fire1");
        }

        /// <summary>
        /// Check if the fire button was pressed this frame.
        /// </summary>
        public bool IsFirePressed()
        {
            return Input.GetButtonDown("Fire1");
        }

        /// <summary>
        /// Check if the aim button is currently held.
        /// </summary>
        public bool IsAiming()
        {
            return Input.GetButton("Fire2");
        }

        /// <summary>
        /// Check if the sprint button is currently held.
        /// </summary>
        public bool IsSprinting()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }

        /// <summary>
        /// Check if the crouch button is currently held.
        /// </summary>
        public bool IsCrouching()
        {
            return Input.GetKey(crouchKey);
        }

        /// <summary>
        /// Remap a key binding at runtime.
        /// </summary>
        public void RemapKey(string action, KeyCode newKey)
        {
            switch (action.ToLower())
            {
                case "interact": interactKey = newKey; break;
                case "altinteract": altInteractKey = newKey; break;
                case "reload": reloadKey = newKey; break;
                case "inventory": inventoryKey = newKey; break;
                case "map": mapKey = newKey; break;
                case "flashlight": flashlightKey = newKey; break;
                case "quicksave": quickSaveKey = newKey; break;
                case "crouch": crouchKey = newKey; break;
            }
        }
    }
}
