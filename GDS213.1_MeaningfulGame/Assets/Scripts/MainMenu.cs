using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] cameras;
    [SerializeField] private float time = 10;

    private int cameraIndex;
    private float timer = -1;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;
        if(cameras.Length > 0)
        {
            for(int i = 0; i < cameras.Length; i++)
            {
                if(i > 0)
                {
                    cameras[i].SetActive(false);
                }
                else
                {
                    cameras[i].SetActive(true);
                }
            }
            timer = 0;
        }
    }

    private void Update()
    {
        if (cameras.Length > 0)
        {
            if (timer >= 0)
            {
                timer += Time.deltaTime;
                if (timer >= time)
                {
                    timer = 0;
                    cameras[cameraIndex].SetActive(false);
                    cameraIndex++;
                    if (cameraIndex >= cameras.Length)
                    {
                        cameraIndex = 0;
                    }
                    cameras[cameraIndex].SetActive(true);
                }
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenFeedbackForm()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSdO79CK3Lz6tcWA_xGOGsn3u74bkoVEpg8HXPv-bzr1sBcuBA/viewform");
    }
}
