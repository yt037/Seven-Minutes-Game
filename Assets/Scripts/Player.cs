// Player.cs
// First person controller and the interaction raycast. Movement and look come
// from Player.inputactions (PlayerControls). Dialogue freezes movement and look;
// E (or click) advances a running dialogue instead of interacting.
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.3f;
    [SerializeField] private Transform playerCamera;

    [Header("Interaction")]
    [Tooltip("The single lever for reach. Raycast length in metres from the camera.")]
    [SerializeField] private float interactDistance = 3f;
    [Tooltip("UI object holding a TMP text; shows the aimed interactable's prompt.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;

    private PlayerControls controls;
    private TMP_Text promptText;
    private Camera cam;

    private float mouseX;
    private float mouseY;
    private bool gameFocused;
    private bool skipLookThisFrame;
    private bool frozen;
    private Vector2 moveInput;
    private Vector2 lookInput;
    [Header("Footsteps")]
    [SerializeField] private float footstepInterval = 0.55f;

    private float footstepTimer;

    public bool Frozen => frozen;

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
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (playerCamera != null) cam = playerCamera.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (playerCamera == null && cam != null) playerCamera = cam.transform;

        if (interactPrompt != null) promptText = interactPrompt.GetComponentInChildren<TMP_Text>(true);

        Focus(true);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (ClueLogUI.IsOpen)
        {
            skipLookThisFrame = true;
            lookInput = Vector2.zero;
            ShowPrompt(null);
            return;
        }

        // Click on the game view to take the cursor back.
        if (!gameFocused && Keys.LeftClickPressed) Focus(true);
        if (!gameFocused) return;

        if (skipLookThisFrame) skipLookThisFrame = false;
        else HandleMouseLook();

        UpdateInteractPrompt();
        UpdateFootsteps();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) { skipLookThisFrame = true; lookInput = Vector2.zero; }
    }

    private void FixedUpdate()
    {
        if (!gameFocused || frozen || DialogueRunning || PauseMenu.IsPaused || ClueLogUI.IsOpen)
        {
            Vector3 stopped = rb.linearVelocity;
            stopped.x = 0f;
            stopped.z = 0f;
            rb.linearVelocity = stopped;
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        movement = Vector3.ClampMagnitude(movement, 1f);
        movement = transform.TransformDirection(movement);

        Vector3 velocity = movement * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    private static bool DialogueRunning => DialogueRunner.Instance != null && DialogueRunner.Instance.IsRunning;

    private void HandleMouseLook()
    {
        mouseX += lookInput.x * mouseSensitivity;
        mouseY -= lookInput.y * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(0f, mouseX, 0f);
        if (playerCamera != null) playerCamera.localRotation = Quaternion.Euler(mouseY, 0f, 0f);
    }

    private void UpdateInteractPrompt()
    {
        Interaction target = Aim(out _);
        ShowPrompt(target != null ? target.Prompt : null);
    }

    private void ShowPrompt(string text)
    {
        if (interactPrompt == null) return;

        bool show = !string.IsNullOrEmpty(text);
        if (interactPrompt.activeSelf != show) interactPrompt.SetActive(show);
        if (show && promptText != null && promptText.text != text) promptText.text = text;
    }

    // Raycast from the crosshair, ignoring trigger volumes (zones and door sensors).
    private Interaction Aim(out RaycastHit hit)
    {
        hit = default;
        if (cam == null) return null;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out hit, interactDistance, ~0, QueryTriggerInteraction.Ignore)) return null;

        return hit.collider.GetComponentInParent<Interaction>();
    }

    private void Interact()
    {
        if (PauseMenu.IsPaused || ClueLogUI.IsOpen) return;

        if (DialogueRunning)
        {
            DialogueRunner.Instance.Advance();
            return;
        }

        if (!gameFocused || frozen) return;

        Interaction target = Aim(out _);
        if (target != null) target.Interact(this);
    }

    private void ToggleCursor() => Focus(!gameFocused);

    private void Focus(bool focus)
    {
        gameFocused = focus;
        skipLookThisFrame = true;
        lookInput = Vector2.zero;

        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !focus;
    }

    // Deaths and endings: no movement, no look, no interaction.
    public void SetFrozen(bool value)
    {
        frozen = value;
        if (value) { moveInput = Vector2.zero; lookInput = Vector2.zero; }
    }
    private void UpdateFootsteps()
{
    if (frozen || DialogueRunning || PauseMenu.IsPaused || ClueLogUI.IsOpen)
    {
        footstepTimer = 0f;
        return;
    }

    bool isMoving = moveInput.sqrMagnitude > 0.01f;

    if (!isMoving)
    {
        footstepTimer = 0f;
        return;
    }

    footstepTimer -= Time.deltaTime;

    if (footstepTimer <= 0f)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(GameIds.SfxFootstep);

        footstepTimer = footstepInterval;
    }
}
}
