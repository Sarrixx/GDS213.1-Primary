using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public delegate void DialogueNodeDelegate(DialogueNode node);
    public delegate void ResponseNodeDelegate(ConversationResponseNode node, DecisionNode characterPrompt);

    [SerializeField] private bool skipAutomaticConversations = true;
    [SerializeField] private ConversationData dayStartConversation;
    [SerializeField] private ConversationData dayEndConversation;

    private int conversationNodeIndex = -1;
    private int selectedDialogueIndex = -1;
    private int totalConversations = 0;
    private int conversationCount = 0;
    //private AudioSource aSrc;

    public int SelectedDialogueIndex
    {
        get { return selectedDialogueIndex; }
        set
        {
            if (CurrentConversation != null && CurrentConversation.Conversation.Length > 0)
            {
                selectedDialogueIndex = value;
            }
        }
    }
    public int ConversationCount { get { return conversationCount; } set { conversationCount = value; } }
    public int TotalConversations { get { return totalConversations; } set { totalConversations = value; } }
    public bool WaitingForResponse { get; private set; } = false;
    public ConversationData CurrentConversation { get; private set; }
    public static Dictionary<string, object> BlackBoard { get; private set; } = new Dictionary<string, object>()
    {
        { "openness", 0f }
    };

    public event DialogueNodeDelegate OnInvokeDialogueNode;
    public event ResponseNodeDelegate OnInvokeResponseNode;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //if(TryGetComponent(out aSrc) == true)
            //{
            //    aSrc.loop = false;
            //}
            if (dayStartConversation.NextTriggers != null && dayStartConversation.NextTriggers.Length > 0)
            {
                foreach (ConversationTrigger trigger in dayStartConversation.NextTriggers)
                {
                    trigger.ConversationData.DisableNextTriggers();
                    trigger.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (skipAutomaticConversations == false)
        {
            if (dayStartConversation.Conversation.Length > 0)
            {
                TotalConversations++;
            }
            if (dayEndConversation.Conversation.Length > 0)
            {
                TotalConversations++;
            }
        }
    }

    public void TriggerDayStart()
    {
        if (dayStartConversation.Conversation.Length > 0 && skipAutomaticConversations == false)
        {
            StartCoroutine(InitiateConversationWithDelay(dayStartConversation, false));
        }
    }

    public bool InitiateConversation(ConversationData conversationData, bool setConversationPostDelay = false)
    {
        if (CurrentConversation == null && conversationNodeIndex <= 0 && conversationData.Conversation.Length > 0)
        {
            StartCoroutine(InitiateConversationWithDelay(conversationData, setConversationPostDelay));
            return true;
        }
        return false;
    }

    private IEnumerator InitiateConversationWithDelay(ConversationData conversationData, bool setConversationPostDelay)
    {
        if (CurrentConversation == null && conversationNodeIndex <= 0 && conversationData.Conversation.Length > 0)
        {
            if (setConversationPostDelay == false)
            {
                conversationNodeIndex = 0;
                CurrentConversation = conversationData;
                if (conversationData.Director != null)
                {
                    conversationData.Director.Play();
                }
                yield return new WaitForSeconds(conversationData.Delay);
            }
            else
            {
                yield return new WaitForSeconds(conversationData.Delay);
                conversationNodeIndex = 0;
                CurrentConversation = conversationData;
                if (conversationData.Director != null)
                {
                    conversationData.Director.Play();
                }
            }
            if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationDialogueNode dialogueNode)
            {
                StartCoroutine(InvokeDialogueNode(dialogueNode.Dialogue));
            }
            else if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationDecisionNode decisionNode)
            {
                StartCoroutine(InvokeDecisionNode(decisionNode.Decisions));
            }
            else if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationResponseNode responseNode)
            {
                StartCoroutine(InvokeResponseNode(responseNode));
            }
        }
    }

    private IEnumerator InvokeDialogueNode(DialogueNode dialogueNode)
    {
        AudioManager.PlayClip(dialogueNode.Audio, dialogueNode.IsPlayer);
        OnInvokeDialogueNode?.Invoke(dialogueNode);
        yield return new WaitForSeconds(dialogueNode.Audio != null && dialogueNode.Duration < dialogueNode.Audio.length ? dialogueNode.Audio.length : dialogueNode.Duration);
        EndDialogue();
    }

    private IEnumerator InvokeDecisionNode(DecisionNode[] decisions)
    {
        DecisionNode selectedPrompt = null;
        float openness = (float)BlackBoard["openness"];
        foreach (DecisionNode prompt in decisions)
        {
            if (selectedPrompt == null) //this section is broken
            {
                selectedPrompt = prompt;
            }
            else if (prompt.Comparitor == DecisionNode.ValueComparitor.greaterThan)
            {
                if (openness >= prompt.Value && selectedPrompt.Value < prompt.Value)
                {
                    selectedPrompt = prompt;
                }
            }
            else if (prompt.Comparitor == DecisionNode.ValueComparitor.lessThan)
            {
                if (openness <= prompt.Value && selectedPrompt.Value > prompt.Value)
                {
                    selectedPrompt = prompt;
                }
            }
        }
        AudioManager.PlayClip(selectedPrompt.Dialogue.Audio, selectedPrompt.Dialogue.IsPlayer);
        OnInvokeDialogueNode?.Invoke(selectedPrompt.Dialogue);
        yield return new WaitForSeconds(selectedPrompt.Dialogue.Audio && selectedPrompt.Dialogue.Duration < selectedPrompt.Dialogue.Audio.length ? selectedPrompt.Dialogue.Audio.length : selectedPrompt.Dialogue.Duration);
        EndDialogue();
    }

    private IEnumerator InvokeResponseNode(ConversationResponseNode node)
    {
        DecisionNode selectedPrompt = null;
        float openness = (float)BlackBoard["openness"];
        foreach (DecisionNode prompt in node.CharacterPrompts)
        {
            if (selectedPrompt == null) //this section is broken
            {
                selectedPrompt = prompt;
            }
            else if (prompt.Comparitor == DecisionNode.ValueComparitor.greaterThan)
            {
                if (openness >= prompt.Value && selectedPrompt.Value < prompt.Value)
                {
                    selectedPrompt = prompt;
                }
            }
            else if (prompt.Comparitor == DecisionNode.ValueComparitor.lessThan)
            {
                if (openness <= prompt.Value && selectedPrompt.Value > prompt.Value)
                {
                    selectedPrompt = prompt;
                }
            }
        }
        AudioManager.PlayClip(selectedPrompt.Dialogue.Audio, selectedPrompt.Dialogue.IsPlayer);
        OnInvokeDialogueNode?.Invoke(selectedPrompt.Dialogue);
        yield return new WaitForSeconds(selectedPrompt.Dialogue.Audio != null && selectedPrompt.Dialogue.Duration < selectedPrompt.Dialogue.Audio.length ? selectedPrompt.Dialogue.Audio.length : selectedPrompt.Dialogue.Duration);
        OnInvokeResponseNode?.Invoke(node, selectedPrompt);
        WaitingForResponse = true;
        while (selectedDialogueIndex < 0)
        {
            yield return new WaitForEndOfFrame();
        }
        WaitingForResponse = false;
        EndDialogue();
    }

    private void EndDialogue()
    {
        //check data event indexes against current node index
        if (selectedDialogueIndex >= 0 && CurrentConversation.Conversation[conversationNodeIndex] is ConversationResponseNode responseNode)
        {
            float openness = (float)BlackBoard["openness"];
            float ratio = responseNode.PlayerResponses[selectedDialogueIndex].ValueModifier / 1;
            Debug.Log("Ratio: " + ratio);
            if (responseNode.PlayerResponses[selectedDialogueIndex].ValueModifier != 0)
            {
                openness += 0.2f * ratio;
                openness = Mathf.Clamp(openness, -1, 1);
                Debug.Log("Openness: " + openness);
                BlackBoard["openness"] = openness;
            }
            StartCoroutine(InvokeDialogueNode(responseNode.PlayerResponses[selectedDialogueIndex]));
            selectedDialogueIndex = -1;
        }
        else
        {
            if (CurrentConversation.NodeEvents != null && CurrentConversation.NodeEvents.Length > 0)
            {
                for(int i = 0; i < CurrentConversation.NodeEvents.Length; i++)
                {
                    if(CurrentConversation.NodeEvents[i].NodeIndex == conversationNodeIndex)
                    {
                        CurrentConversation.NodeEvents[i].Events.Invoke();
                        break;
                    }
                }
            }
            conversationNodeIndex++;
            if (conversationNodeIndex < CurrentConversation.Conversation.Length)
            {
                if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationDialogueNode dialogueNode)
                {
                    StartCoroutine(InvokeDialogueNode(dialogueNode.Dialogue));
                }
                else if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationDecisionNode decisionNode)
                {
                    StartCoroutine(InvokeDecisionNode(decisionNode.Decisions));
                }
                else if (CurrentConversation.Conversation[conversationNodeIndex] is ConversationResponseNode response)
                {
                    StartCoroutine(InvokeResponseNode(response));
                }
            }
            else
            {
                CurrentConversation.ActivateNextTriggers();
                CurrentConversation = null;
                selectedDialogueIndex = -1;
                conversationNodeIndex = -1;
                OnInvokeDialogueNode.Invoke(null);
                ConversationCount++;
                if (ConversationCount >= TotalConversations)
                {
                    if(DayManager.Instance != null && DayManager.Instance.TriggerNextDay() == 0)
                    {
                        BlackBoard["openness"] = 0f;
                        Debug.Log("GAME FINISHED");
                    }
                    else
                    {
                        Debug.Log("LOAD NEXT SCENE");
                    }
                }
                else if (dayEndConversation.Conversation.Length > 0 && ConversationCount == TotalConversations - 1 && skipAutomaticConversations == false)
                {
                    StartCoroutine(InitiateConversationWithDelay(dayEndConversation, true));
                }
            }
        }
    }
}