using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private bool active = true;
    [SerializeField] private bool ignoreActiveDialogue = false;
    [Tooltip("The audio clip that plays on interaction.")]
    [SerializeField] private AudioClip interactionClip;

    protected AudioSource aSrc;

    protected AudioClip InteractionClip { get { return interactionClip; } }
    public bool Active { get { return active; } set { active = value; } }
    public bool IgnoreDialogue { get { return ignoreActiveDialogue; }}

    protected virtual void Awake()
    {
        TryGetComponent(out aSrc);
    }

    public virtual bool OnInteract(InteractionHitInfo interactionInfo)
    {
        if (active == true)
        {
            PlaySound(interactionClip, aSrc);
            return true;
        }
        return false;
    }

    protected bool PlaySound(AudioClip clip, AudioSource source, float volScale = 1f)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip, volScale);
            return true;
        }
        return false;
    }

    public void ToggleIgnoreDialogue(bool toggle)
    {
        ignoreActiveDialogue = toggle;
    }
}