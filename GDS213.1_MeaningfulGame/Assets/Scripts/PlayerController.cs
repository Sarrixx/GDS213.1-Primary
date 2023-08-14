using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Physics Properties")]
    [Tooltip("Defines how strong gravity is.")]
    [Range(1f, 100f)][SerializeField] private float gravity;
    [Header("Movement Properties")]
    [Tooltip("Defines the default speed at which the player will move.")]
    [Range(1f, 10f)][SerializeField] private float defaultSpeed;
    [SerializeField] private bool movementDisabledOnStart = true;

    private CharacterController controller;
    private Vector3 motionStep;
    private float velocity = 0f;

    /// <summary>
    /// Enables/disables the ability to move the player character by device input.
    /// </summary>
    public bool MovementEnabled { get; set; } = true;

    public static PlayerController Instance { get; private set; }

    /// <summary>
    /// Awake is called before Start.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (TryGetComponent(out controller) == true)
            {
            }
            CameraFeed.FeedToggledEvent += ToggleMovement;
            if(movementDisabledOnStart == true)
            {
                MovementEnabled = false;
            }
        }
    }

    private bool ToggleMovement(bool toggle)
    {
        if(toggle == true && MovementEnabled == true)
        {
            MovementEnabled = false;
            return true;
        }
        else if(toggle == false && MovementEnabled == false)
        {
            MovementEnabled = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// FixedUpdate may be called more than once per frame.
    /// </summary>
    private void FixedUpdate()
    {
        if (controller.isGrounded == true)
        {
            velocity = -gravity * Time.deltaTime;
        }
        else
        {
            velocity -= gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (MovementEnabled == true)
        {
            ApplyMovement();
        }
        else
        {
            ApplyMovement(true);
        }
    }

    /// <summary>
    /// Moves the character based on the input detected every frame.
    /// </summary>
    private void ApplyMovement(bool ignoreInputs = false)
    {
        motionStep = Vector3.zero;
        if (ignoreInputs == false)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            motionStep += transform.forward * verticalInput;
            motionStep += transform.right * horizontalInput;
            motionStep = defaultSpeed * motionStep.normalized;
        }
        motionStep.y += velocity;
        controller.Move(motionStep * Time.deltaTime);
    }

    /// <summary>
    /// Forces the character to move to a position.
    /// </summary>
    /// <param name="position"></param>
    public void TeleportToPosition(Vector3 position)
    {
        controller.enabled = false;
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        transform.position = position;
        controller.enabled = true;
    }
}