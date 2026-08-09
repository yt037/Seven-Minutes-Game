using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float mouseSensitivity = 10f;
    private float mouseX;
    private float mouseY;
    private PlayerControls controls;
    private bool gameFocused = false;

    public bool dialogueOpen;
    [SerializeField] private GameObject dialogue;
    
    [SerializeField] private float interactDistance = 3f;

    

    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.ToggleCursor.performed += ctx => ToggleCursor();
        controls.Player.Interact.performed += ctx => Interact();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            gameFocused = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!gameFocused)
            return;

        // Movement
        Vector3 movement = new Vector3(
            moveInput.x,
            0,
            moveInput.y
        );

        transform.Translate(
            movement * moveSpeed * Time.deltaTime
        );

        // Mouse look
        mouseX += lookInput.x * mouseSensitivity;
        mouseY -= lookInput.y * mouseSensitivity;

        mouseY = Mathf.Clamp(mouseY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(
            mouseY,
            mouseX,
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
        {
            return;
        }

        if (dialogue.activeSelf)
        {
            dialogue.SetActive(false);
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.TryGetComponent(out Card keycard))
            {
                keycard.Collect(this);
            }
            else if (hit.collider.TryGetComponent(out Guard guard))
            {
                guard.Interact(this);
                dialogue.SetActive(true);
            }
        }
    }
}
