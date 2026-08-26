using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private Transform playerCamera;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;

    [SerializeField] private DialogueManager dialogue;

    private float mouseX;
    private float mouseY;
    private PlayerControls controls;
    private bool gameFocused = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private Transform playerCamera;

    [Header("Interaction")]
    [SerializeField] private GameObject dialogue;
    [SerializeField] private float interactDistance = 3f;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;

    public bool dialogueOpen;

    private float mouseX;
    private float mouseY;

    private PlayerControls controls;
    private bool gameFocused = false;

    private Vector2 moveInput;
    private Vector2 lookInput;


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx =>
            moveInput = ctx.ReadValue<Vector2>();

        controls.Player.Move.canceled += ctx =>
            moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx =>
            lookInput = ctx.ReadValue<Vector2>();

        controls.Player.Look.canceled += ctx =>
            lookInput = Vector2.zero;

        controls.Player.ToggleCursor.performed += ctx =>
            ToggleCursor();

        controls.Player.Interact.performed += ctx =>
            Interact();
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Make sure the Rigidbody reference exists.
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Prevent physics from tipping the player over.
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }


    private void OnEnable()
    {
        controls.Enable();
    }


    private void OnDisable()
    {
        controls.Disable();
    }


    private void Update()
    {
        // Click the game to focus it.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            gameFocused = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!gameFocused)
            return;

        HandleMouseLook();
    }


    private void FixedUpdate()
    {
        if (!gameFocused)
        {
            // Stop horizontal movement when the game isn't focused.
            Vector3 stoppedVelocity = rb.linearVelocity;
            stoppedVelocity.x = 0f;
            stoppedVelocity.z = 0f;
            rb.linearVelocity = stoppedVelocity;

            return;
        }

        HandleMovement();
    }


    private void HandleMovement()
    {
        // Convert WASD input into a 3D direction.
        Vector3 movement = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        // Prevent diagonal movement from being faster.
        movement = Vector3.ClampMagnitude(movement, 1f);

        // Make movement relative to where the player is facing.
        movement = transform.TransformDirection(movement);

        // Keep the Rigidbody's current vertical velocity.
        // This allows gravity to continue working.
        Vector3 velocity = movement * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }


    private void HandleMouseLook()
    {
        // Horizontal mouse movement rotates the player.
        mouseX += lookInput.x * mouseSensitivity;

        // Vertical mouse movement rotates the camera.
        mouseY -= lookInput.y * mouseSensitivity;

        mouseY = Mathf.Clamp(mouseY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(
            0f,
            mouseX,
            0f
        );

        playerCamera.localRotation = Quaternion.Euler(
            mouseY,
            0f,
            0f
        );
    }


    private void ToggleCursor()
    {
        gameFocused = !gameFocused;

        if (gameFocused)
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


    private void Interact()
    {
        if (!gameFocused)
            return;


        if (dialogue != null && dialogue.activeSelf)
        {
            dialogue.CloseDialogue();
        }

        Camera cam = playerCamera.GetComponent<Camera>();

        if (cam == null)
            cam = Camera.main;

        Ray ray = cam.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance))
        {
            if (hit.collider.TryGetComponent(out Interaction interactable))
            {
                interactable.Interact(this);

                if (dialogue != null)
                {
                    dialogue.SetActive(true);
                }
            }
        }
    }
}

