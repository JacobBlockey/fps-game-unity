using UnityEngine;

namespace ZONE.Systems
{
    /// <summary>
    /// Raycast-based interaction system. Detects interactable objects in front of the player,
    /// highlights them, and shows context prompts. Handles key-based interaction.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private float detectionRadius = 0.1f;
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("Keybinds")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode altInteractKey = KeyCode.F; // Secondary action (pick up, inspect)

        [Header("UI")]
        [SerializeField] private GameObject interactionPromptPrefab;
        [SerializeField] private Transform promptAnchor; // Where to position the prompt (usually center of screen)

        private IInteractable _currentTarget;
        private GameObject _currentPrompt;

        public IInteractable CurrentTarget => _currentTarget;

        public delegate void TargetChangedHandler(IInteractable newTarget);
        public event TargetChangedHandler OnTargetChanged;

        public delegate void InteractionHandler(IInteractable target, bool isAlt);
        public event InteractionHandler OnInteraction;

        private void Update()
        {
            DetectInteractable();
            HandleInput();
        }

        private void DetectInteractable()
        {
            IInteractable newTarget = null;

            // Primary raycast from camera center
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, detectionRadius, interactionRange, interactableLayers);

            foreach (RaycastHit hit in hits)
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    newTarget = interactable;
                    break;
                }
            }

            // Check if target changed
            if (newTarget != _currentTarget)
            {
                _currentTarget = newTarget;
                OnTargetChanged?.Invoke(newTarget);

                // Update UI prompt
                UpdatePrompt(newTarget);
            }
        }

        private void HandleInput()
        {
            if (_currentTarget == null) return;

            if (Input.GetKeyDown(interactKey))
            {
                _currentTarget.Interact(gameObject);
                OnInteraction?.Invoke(_currentTarget, false);
            }

            if (Input.GetKeyDown(altInteractKey))
            {
                // Alt interaction (pick up, inspect, etc.)
                if (_currentTarget is IAltInteractable altInteractable)
                {
                    altInteractable.AltInteract(gameObject);
                    OnInteraction?.Invoke(_currentTarget, true);
                }
                else
                {
                    // Default alt behavior: pick up
                    _currentTarget.Interact(gameObject);
                    OnInteraction?.Invoke(_currentTarget, true);
                }
            }
        }

        private void UpdatePrompt(IInteractable target)
        {
            // In full implementation, show/hide a world-space or screen space prompt
            // with the interaction text (e.g., "[E] Open Door", "[F] Pick Up")
            if (target != null)
            {
                Debug.Log($"[Interaction] Looking at: {target.InteractionText}");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (cameraTransform == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);

            if (_currentTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(
                    cameraTransform.position + cameraTransform.forward * interactionRange,
                    detectionRadius
                );
            }
        }
#endif
    }

    /// <summary>
    /// Interface for any object the player can interact with.
    /// </summary>
    public interface IInteractable
    {
        bool CanInteract { get; }
        string InteractionText { get; }
        void Interact(GameObject interactor);
    }

    /// <summary>
    /// Optional secondary interaction (inspect, pick up, etc.)
    /// </summary>
    public interface IAltInteractable
    {
        string AltInteractionText { get; }
        void AltInteract(GameObject interactor);
    }

    /// <summary>
    /// Example: A lootable container (chest, stash, corpse).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LootContainer : MonoBehaviour, IInteractable, IAltInteractable
    {
        [SerializeField] private string containerName = "Container";
        [SerializeField] private bool isOpened = false;
        [SerializeField] private ItemDefinition[] contents;
        [SerializeField] private int[] quantities;

        public bool CanInteract => !isOpened;
        public string InteractionText => $"[E] Open {containerName}";
        public string AltInteractionText => "[F] Quick Loot";

        public void Interact(GameObject interactor)
        {
            if (isOpened) return;
            isOpened = true;

            var inventory = interactor.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                for (int i = 0; i < contents.Length; i++)
                {
                    int qty = i < quantities.Length ? quantities[i] : 1;
                    inventory.AddItem(contents[i], qty);
                }
            }

            Debug.Log($"[LootContainer] Opened {containerName} — {contents.Length} item types found");
        }

        public void AltInteract(GameObject interactor)
        {
            // Quick loot: take everything without opening UI
            Interact(interactor);
        }
    }

    /// <summary>
    /// Example: A door that opens when interacted with.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedMessage = "This door is locked.";
        [SerializeField] private Animator doorAnimator;

        public bool CanInteract => true;
        public string InteractionText => isLocked ? "[E] Locked" : (isOpen ? "[E] Close" : "[E] Open");

        public void Interact(GameObject interactor)
        {
            if (isLocked)
            {
                Debug.Log($"[Door] {lockedMessage}");
                return;
            }

            isOpen = !isOpen;
            if (doorAnimator) doorAnimator.SetBool("IsOpen", isOpen);
            Debug.Log($"[Door] Door {(isOpen ? "opened" : "closed")}");
        }
    }
}
