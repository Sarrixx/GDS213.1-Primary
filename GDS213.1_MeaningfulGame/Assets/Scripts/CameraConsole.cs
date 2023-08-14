using UnityEngine;

public class CameraConsole : Interactable
{
    [SerializeField] private CameraFeed[] cameras;
    [SerializeField] private ConversationData interactionConversation;

    private float transitionTime = 0.25f;
    private int currentFeedIndex = 0;
    private float transitionTimer = -1;
    private bool consoleEngaged = false;

    public ConversationData InteractionConversation { get { return interactionConversation; } }

    protected override void Awake()
    {
        base.Awake();
        if (interactionConversation.NextTriggers != null && interactionConversation.NextTriggers.Length > 0)
        {
            foreach (ConversationTrigger trigger in interactionConversation.NextTriggers)
            {
                trigger.ConversationData.DisableNextTriggers();
                trigger.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        if(interactionConversation != null && interactionConversation.Conversation.Length > 0)
        {
            DialogueManager.Instance.TotalConversations++;
        }
    }

    private void Update()
    {
        if (consoleEngaged == true && transitionTimer < 0)
        {
            if (cameras[currentFeedIndex].Zoomed == false)
            {
                if (Time.timeScale == 1)
                {
                    if (Input.GetAxisRaw("Change Camera Feed") > 0 || Input.mouseScrollDelta.y > 0 && DialogueManager.Instance.WaitingForResponse == false)
                    {
                        cameras[currentFeedIndex].Deactivate();
                        currentFeedIndex++;
                        if (currentFeedIndex >= cameras.Length)
                        {
                            currentFeedIndex = 0;
                        }
                        cameras[currentFeedIndex].Activate();
                        transitionTimer = 0;
                    }
                    else if (Input.GetAxisRaw("Change Camera Feed") < 0 || Input.mouseScrollDelta.y < 0 && DialogueManager.Instance.WaitingForResponse == false)
                    {
                        cameras[currentFeedIndex].Deactivate();
                        currentFeedIndex--;
                        if (currentFeedIndex < 0)
                        {
                            currentFeedIndex = cameras.Length - 1;
                        }
                        cameras[currentFeedIndex].Activate();
                        transitionTimer = 0;
                    }
                }
            }
        }

        if(transitionTimer >= 0 )
        {
            transitionTimer += Time.deltaTime;
            if(transitionTimer > transitionTime)
            {
                transitionTimer = -1;
            }
        }
    }

    public override bool OnInteract(InteractionHitInfo interactionInfo)
    {
        if(cameras[currentFeedIndex].Zoomed == true)
        {
            return false;
        }
        if (consoleEngaged == false)
        {
            consoleEngaged = true;
            cameras[currentFeedIndex].Activate();
            if(interactionConversation != null)
            {
                DialogueManager.Instance.InitiateConversation(interactionConversation, false);
                interactionConversation = null;
            }
            return true;
        }
        else
        {
            consoleEngaged = false;
            cameras[currentFeedIndex].Deactivate();
            return false;
        }
    }
}
