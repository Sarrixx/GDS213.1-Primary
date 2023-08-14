using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFeed : MonoBehaviour
{
    public delegate bool CameraFeedToggleDelegate(bool toggle);

    [SerializeField] private float speed;
    [SerializeField] private float interactionDistance;
    [SerializeField] private float clampBounds;
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private Animator dialogueUIAnim;
    [SerializeField] private AudioClip moveAudio;
    [SerializeField] private AudioClip endMovingAudio;
    [SerializeField] private AudioSource aSrc;

    private Vector3 originalRot;
    private float currentYOffset;
    private float currentXOffset;
    private ZoomController zoom;

    public bool Zoomed { get { return zoom.Zoomed; } }

    public static event CameraFeedToggleDelegate FeedToggledEvent;

    private void Awake()
    {
        TryGetComponent(out zoom);
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRot = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        float yRotationStep = horizontalInput * speed * Time.deltaTime;
        float xRotationStep = verticalInput * speed * Time.deltaTime;
        currentYOffset += yRotationStep;
        currentXOffset += xRotationStep;
        if((horizontalInput != 0 && currentYOffset > -clampBounds && currentYOffset < clampBounds) || (verticalInput != 0 && currentXOffset > -clampBounds && currentXOffset < clampBounds))
        {
            if(aSrc.clip != moveAudio)
            {
                aSrc.clip = moveAudio;
                aSrc.loop = true;
                aSrc.Play();
            }
        }
        else if(aSrc.clip == moveAudio)
        {
            aSrc.Stop();
            aSrc.clip = null;
            aSrc.loop = false;
            aSrc.PlayOneShot(endMovingAudio);
        }
        currentYOffset = Mathf.Clamp(currentYOffset, -clampBounds, clampBounds);
        currentXOffset = Mathf.Clamp(currentXOffset, -clampBounds, clampBounds);
        transform.localRotation = Quaternion.Euler(originalRot.x - currentXOffset, originalRot.y + currentYOffset, transform.localEulerAngles.z);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore) == true)
        {
            Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.green);
            if (dialogueUIAnim.GetBool("promptDialogue") == false && DialogueManager.Instance.CurrentConversation == null)
            {
                dialogueUIAnim.SetBool("promptDialogue", true);
            }
            //if input received, trigger conversation
            if(Input.GetButtonDown("Trigger Conversation") == true)
            {
                if(hit.collider.TryGetComponent(out ConversationTrigger trigger) == true)
                {
                    if(DialogueManager.Instance.InitiateConversation(trigger.ConversationData) == true)
                    {
                        trigger.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.red);
            if (dialogueUIAnim.GetBool("promptDialogue") == true)
            {
                dialogueUIAnim.SetBool("promptDialogue", false);
            }
        }
    }

    public bool Activate()
    {
        if(FeedToggledEvent.Invoke(true) == true)
        {
            gameObject.SetActive(true);
            return true;
        }
        return false;
    }

    public bool Deactivate()
    {
        if (FeedToggledEvent.Invoke(false) == true)
        {
            gameObject.SetActive(false);
            if (aSrc.clip == moveAudio)
            {
                aSrc.Stop();
                aSrc.clip = null;
                aSrc.loop = false;
            }
            if (dialogueUIAnim.GetBool("promptDialogue") == true)
            {
                dialogueUIAnim.SetBool("promptDialogue", false);
            }
            return true;
        }
        return false;
    }
}
