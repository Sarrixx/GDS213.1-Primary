using UnityEngine;

public class ZoomController : MonoBehaviour
{
    [Tooltip("Defines how far the field of view zooms. Pass a negative number to zoom out.")]
    [Range(1, 5)][SerializeField] private float zoomMultiplier;
    [Tooltip("The time it takes to zoom.")]
    [Range(0.01f, 2f)][SerializeField] private float zoomTime;
    [Tooltip("The audio clip that plays when zooming is activated.")]
    [SerializeField] private AudioClip activateClip;
    [Tooltip("The audio clip that plays when zooming is deactivated.")]
    [SerializeField] private AudioClip deactivateClip;
    [SerializeField] private AudioSource aSrc;

    private float defaultFOV = 60;
    private float targetFOV = 0;
    private float currentFOV = 0;
    private float timer = -1;
    private Camera cam;

    public bool Zoomed { get; private set; } = false;

    private void Awake()
    {
        if (TryGetComponent(out cam) == true)
        {
            defaultFOV = cam.fieldOfView;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Zoom") == true)
        {
            Activate();
        }
        else if (Input.GetButtonUp("Zoom") == true)
        {
            Deactivate();
        }

        if (timer >= 0)
        {
            timer += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(currentFOV, targetFOV, timer / zoomTime);
            if (timer > zoomTime)
            {
                if (targetFOV == defaultFOV)
                {
                    Zoomed = false;
                }
                timer = -1;
            }
        }
    }

    private void Activate()
    {
        timer = 0;
        targetFOV = defaultFOV / zoomMultiplier;
        currentFOV = cam.fieldOfView;
        if (aSrc != null && activateClip != null)
        {
            aSrc.clip = activateClip;
            aSrc.Play();
        }
        Zoomed = true;
    }

    private void Deactivate()
    {
        timer = 0;
        targetFOV = defaultFOV;
        currentFOV = cam.fieldOfView;
        if (aSrc != null && deactivateClip != null)
        {
            aSrc.clip = deactivateClip;
            aSrc.Play();
        }
    }
}