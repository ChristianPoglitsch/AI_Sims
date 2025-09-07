using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private CharacterController controller;

    private Vector2 moveInput;
    private Vector2 lookInput;

    public float moveSpeed = 5f;
    public float lookSpeed = 100f;
    public float gravity = -9.81f;

    private Vector3 velocity;

    // Reference to the main camera
    private Transform camTransform;
    private float pitch = 0f;

    // Camera height offset (like head height)
    public float cameraHeight = 1.0f;

    public ConversationManager conversationManager;

    // --- NEW: toggle look ---
    private bool allowLook = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        camTransform = Camera.main.transform;
    }

    public void OnFire()
    {
        Debug.Log("Fire pressed → checking for NPC...");

        // Raycast from center of screen
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            NPCToStoryBridge npcBridge = hit.collider.GetComponent<NPCToStoryBridge>();
            if (npcBridge != null)
            {
                Debug.Log("Hit NPC: " + hit.collider.name);

                // Make NPC look at the player (horizontal only)
                Vector3 lookTarget = transform.position;
                lookTarget.y = hit.collider.transform.position.y;
                hit.collider.transform.LookAt(lookTarget);

                // Pass bridge to conversation manager
                if (conversationManager != null)
                {
                    conversationManager.SetCurrentNPC(npcBridge);
                    conversationManager.Talk();
                }
            }
        }
    }

    void Update()
    {
        if (camTransform == null) return;

        // --- INPUT ---
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();

        // --- Toggle look on RMB press ---
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            allowLook = !allowLook;
            Debug.Log("Camera look toggled: " + allowLook);
        }

        // --- MOVEMENT ---
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * moveInput.y + right * moveInput.x;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- ROTATION (only if enabled) ---
        if (allowLook)
        {
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed * Time.deltaTime);

            pitch -= lookInput.y * lookSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            camTransform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
        }

        // --- CAMERA FOLLOW ---
        camTransform.position = transform.position + new Vector3(0, cameraHeight, 0);
    }
}
