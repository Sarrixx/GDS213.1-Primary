using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class UIManager_Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseBGReference;

    private bool previousLookState = false;
    private PostProcessVolume volume;
    private Bloom bloom;
    private DepthOfField dof;
    private MotionBlur blur;

    private void Awake()
    {
        GameObject.Find("CameraPost").TryGetComponent(out volume);
        if(volume != null)
        {
            if(volume.profile.TryGetSettings(out bloom) == true)
            {
                Debug.Log("Found Bloom");
            }
            volume.profile.TryGetSettings(out blur);
            volume.profile.TryGetSettings(out dof);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        TogglePause(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel") == true )
        {
            if(pauseBGReference != null)
            {
                TogglePause(!pauseBGReference.activeSelf);
            }
        }
    }

    public bool TogglePause(bool toggle)
    {
        if(pauseBGReference != null)
        {
            if (pauseBGReference.activeSelf == true)
            {
                if (toggle == false)
                {
                    Time.timeScale = 1;
                    AudioListener.pause = false;
                    pauseBGReference.SetActive(false);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    if (MouseLook.Instance != null)
                    {
                        MouseLook.Instance.LookEnabled = previousLookState;
                    }
                    return true;
                }
            }
            else if (pauseBGReference.activeSelf == false)
            {
                if (toggle == true)
                {
                    Time.timeScale = 0;
                    AudioListener.pause = true;
                    pauseBGReference.SetActive(true);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    if (MouseLook.Instance != null)
                    {
                        previousLookState = MouseLook.Instance.LookEnabled;
                        MouseLook.Instance.LookEnabled = false;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void Resume()
    {
        TogglePause(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        Destroy(transform.root.gameObject);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToggleBloom()
    {
        bloom.active = !bloom.active;
    }

    public void ToggleBlur()
    {
        blur.active = !blur.active;
    }

    public void ToggleDepthOfField()
    {
        dof.active = !dof.active;
    }
}
