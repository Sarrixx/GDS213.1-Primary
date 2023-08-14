using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioSource radioSource;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            playerSource.loop = false;
            radioSource.loop = false;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    public static bool PlayClip(AudioClip clip, bool isPlayer)
    {
        if (Instance != null)
        {
            if (isPlayer == true)
            {
                Instance.playerSource.clip = clip;
                if (clip != null)
                {
                    Instance.playerSource.Play();
                }
            }
            else
            {
                Instance.radioSource.clip = clip;
                if (clip != null)
                {
                    Instance.radioSource.Play();
                }
            }
            return true;
        }
        return false;
    }
}
