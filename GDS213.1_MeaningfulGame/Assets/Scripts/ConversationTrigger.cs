using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    [SerializeField] private ConversationData conversationData;

    private bool tracked = false;

    public ConversationData ConversationData { get { return conversationData; } }

    private void Awake()
    {
        if(conversationData != null)
        {
            if(conversationData.NextTriggers != null && conversationData.NextTriggers.Length > 0)
            {
                foreach(ConversationTrigger trigger in conversationData.NextTriggers)
                {
                    trigger.conversationData.DisableNextTriggers();
                    trigger.gameObject.SetActive(false);
                }
            }
        }
    }

    private void Start()
    {
        TrackConversation();
    }

    public void Track()
    {
        TrackConversation();
    }

    public bool TrackConversation()
    {
        if (tracked == false && conversationData != null && conversationData.Conversation.Length > 0)
        {
            DialogueManager.Instance.TotalConversations++;
            tracked = true;
            return true;
        }
        return false;
    }
}
