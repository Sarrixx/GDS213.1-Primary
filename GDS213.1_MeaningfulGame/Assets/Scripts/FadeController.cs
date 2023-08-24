using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float targetVolume;

    private AudioSource aSrc;
    private float startVolume = 0;
    private float volumeTarget = 0;
    private float fadeTime = 0;
    private float timer = -1;

    private void Awake()
    {
        TryGetComponent(out aSrc);
    }

    private void Start()
    {
        if(aSrc != null && aSrc.playOnAwake == true)
        {
            FadeIn(2f);
        }
    }

    void Update()
    {
        if(timer >= 0 && fadeTime > 0)
        {
            timer += Time.deltaTime;
            aSrc.volume = Mathf.Lerp(startVolume, volumeTarget, timer / fadeTime);
            if(timer > fadeTime)
            {
                timer = -1;
                fadeTime = 0;
                volumeTarget = 0;
            }
        }
    }


    public void FadeIn(float time)
    {
        if(time > 0 && aSrc != null)
        {
            startVolume = aSrc.volume;
            volumeTarget = targetVolume;
            fadeTime = time;
            timer = 0;
        }
    }

    public void FadeOut(float time)
    {
        if(time > 0 && aSrc != null)
        {
            startVolume = aSrc.volume;
            volumeTarget = 0;
            fadeTime = time;
            timer = 0;
        }
    }
}
