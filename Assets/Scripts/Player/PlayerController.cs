using UnityEngine;

namespace ZONE.Player
{
    /// <summary>
    /// Core first-person player controller.
    /// Handles movement, look, sprint, crouch, and stamina.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 85f;

        [Header("Crouch")]
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 15f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float staminaRegenDelay = 2f;

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _xRotation;
        private float _currentStamina;
        private float _staminaRegenTimer;
        private bool _isCrouching;
        private bool _isSprinting;
        private float _currentHeight;
        private float _targetHeight;

        // Encumbrance multiplier (set by inventory system)
        public float EncumbranceMultiplier { get; set; } = 1f;

        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsSprinting => _isSprinting;
        public bool IsCrouching => _isCrouching;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _currentStamina = maxStamina;
            _currentHeight = standingHeight;
            _targetHeight = standingHeight;

            LockCursor();
        }

        private void Update()
        {
            HandleLook();
            HandleCrouch();
            HandleMovement();
            HandleStamina();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -maxLookAngle, maxLookAngle);

            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleCrouch()
        {
            _isCrouching = Input.GetKey(KeyCode.LeftControl);

            _targetHeight = _isCrouching ? crouchHeight : standingHeight;
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
            _controller.height = _currentHeight;
            _controller.center = new Vector3(0f, _currentHeight / 2f, 0f);
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move = move.normalized;

            float speed = GetMovementSpeed();
            _controller.Move(move * speed * EncumbranceMultiplier * Time.deltaTime);

            // Jump
            if (Input.GetButtonDown("Jump") && _controller.isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        private float GetMovementSpeed()
        {
            _isSprinting = Input.GetKey(KeyCode.LeftShift) && !_isCrouching && _currentStamina > 0f;

            if (_isCrouching) return crouchSpeed;
            if (_isSprinting) return sprintSpeed;
            return walkSpeed;
        }

        private void HandleStamina()
        {
            if (_isSprinting && _controller.velocity.magnitude > 0.1f)
            {
                _currentStamina -= staminaDrainRate * Time.deltaTime;
                _staminaRegenTimer = 0f;
            }
            else
            {
                _staminaRegenTimer += Time.deltaTime;
                if (_staminaRegenTimer >= staminaRegenDelay)
                {
                    _currentStamina += staminaRegenRate * Time.deltaTime;
                }
            }

            _currentStamina = Mathf.Clamp(_currentStamina, 0f, maxStamina);
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
