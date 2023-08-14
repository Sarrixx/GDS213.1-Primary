using System.Collections;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Tooltip("Controls how much the camera will move in response to detected mouse input.")]
    [SerializeField] private float sensitivity = 2;
    [Tooltip("Controls how much the camera will continue to move after mouse input ceases.")]
    [SerializeField] private float drag = 5;
    [SerializeField][Range(-10, -90)] private float verticalClampMin = -70f;
    [SerializeField][Range(10, 90)] private float verticalClampMax = 70f;
    [Header("Camera Sway")]
    [SerializeField][Min(0)] private float swaySensitivity = 1f;
    [SerializeField][Range(0.1f, 10)] private float cameraSwaySpeed = 1f;
    [SerializeField][Range(-25f, -1)] private float swayClampMin = -10f;
    [SerializeField][Range(1f, 25)] private float swayClampMax = 10f;
    [SerializeField] private bool lookDisabledOnStart = true;

    private Camera cam;
    private AudioListener listener;
    private Transform character;
    private Coroutine timedShakeRoutine;
    private Vector2 mouseDir;
    private Vector2 smoothing;
    private Vector2 result;
    private Vector3 originalLocalPosition;
    private bool cameraShaking = false;
    private float currentShakeMagnitude = 0f;
    private float currentSwayAmount = 0f;

    /// <summary>
    /// Enables/disables the ability to move the camera by moving the mouse.
    /// </summary>
    public bool LookEnabled { get; set; } = true;
    /// <summary>
    /// Enables/disables the swap applied to camera movement.
    /// </summary>
    public bool SwayEnabled { get; set; } = true;

    public static MouseLook Instance { get; private set; }

    /// <summary>
    /// Awake is called before Start.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            TryGetComponent(out cam);
            TryGetComponent(out listener);
            character = transform.root;
            cameraShaking = false;
            originalLocalPosition = transform.localPosition;
            AlignForwardWithCharacter();
            CameraFeed.FeedToggledEvent += ToggleMouseLook;
            if (lookDisabledOnStart == true)
            {
                LookEnabled = false;
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool ToggleMouseLook(bool toggle)
    {
        if (toggle == true && LookEnabled == true)
        {
            LookEnabled = false;
            if (cam != null)
            {
                cam.enabled = false;
            }
            if (listener != null)
            {
                listener.enabled = false;
            }
            return true;
        }
        else if (toggle == false && LookEnabled == false)
        {
            LookEnabled = true;
            if (cam != null)
            {
                cam.enabled = true;
            }
            if (listener != null)
            {
                listener.enabled = true;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if (LookEnabled == true)
        {
            mouseDir = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseDir *= sensitivity;
            smoothing = Vector2.Lerp(smoothing, mouseDir, 1 / drag);
            result += smoothing;
            result.y = Mathf.Clamp(result.y, verticalClampMin, verticalClampMax);
            SetRotation(result.x, -result.y);

            ApplyCameraSway();
        }

        if (cameraShaking == true)
        {
            float x = Random.Range(-0.1f, 0.1f) * currentShakeMagnitude;
            float y = Random.Range(-0.1f, 0.1f) * currentShakeMagnitude;

            transform.localPosition = new Vector3(originalLocalPosition.x + x, originalLocalPosition.y + y, originalLocalPosition.z);
        }
    }

    private void ApplyCameraSway()
    {
        if (SwayEnabled == true)
        {
            float targetSwayAmount = -mouseDir.x * swaySensitivity;
            currentSwayAmount = Mathf.Lerp(currentSwayAmount, targetSwayAmount, Time.deltaTime * cameraSwaySpeed);
            currentSwayAmount = Mathf.Clamp(currentSwayAmount, swayClampMin, swayClampMax);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentSwayAmount);
        }
    }

    private void SetRotation(float characterYRot, float cameraXRot, bool lookEnabled = true)
    {
        LookEnabled = false;
        character.rotation = Quaternion.AngleAxis(characterYRot, character.up);
        transform.localRotation = Quaternion.AngleAxis(cameraXRot, Vector3.right);
        LookEnabled = lookEnabled;
    }

    private IEnumerator ShakeForTime(float time, float magnitude)
    {
        ToggleCameraShake(true, magnitude);
        yield return new WaitForSeconds(time);
        ToggleCameraShake(false);
        timedShakeRoutine = null;
    }

    public void OverrideRotation(Vector3 direction, float cameraOffset, bool lookEnabled)
    {
        Vector3 flattenedDirection = new Vector3(direction.x, direction.y, direction.z).normalized;
        float characterRotation = Mathf.Atan2(flattenedDirection.x, flattenedDirection.z) * Mathf.Rad2Deg;
        result = new Vector2(characterRotation, result.y + cameraOffset);
        SetRotation(characterRotation, -result.y, lookEnabled);
    }

    public void AlignForwardWithCharacter()
    {
        OverrideRotation(character.forward, 0, true);
    }

    public void ToggleCameraShake(bool toggle, float magnitude = 1f)
    {
        cameraShaking = toggle;
        currentShakeMagnitude = magnitude;
    }

    public void TimedCameraShake(float time, float magnitude)
    {
        if (timedShakeRoutine != null)
        {
            StopCoroutine(timedShakeRoutine);
        }
        timedShakeRoutine = StartCoroutine(ShakeForTime(time, magnitude));
    }
}