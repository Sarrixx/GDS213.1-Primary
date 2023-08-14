using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Conversation Nodes/Dialogue Node")]
public class ConversationDialogueNode : ConversationNode
{
    [SerializeField] private DialogueNode dialogue;

    public DialogueNode Dialogue { get { return dialogue; } }
}