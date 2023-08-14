using UnityEngine;

[CreateAssetMenu(fileName = "New Response Node", menuName = "Conversation Nodes/Response Node")]
public class ConversationResponseNode : ConversationNode
{
    [SerializeField] private DecisionNode[] characterPrompts;
    [SerializeField] private DialogueResponseNode[] playerResponses;

    public DecisionNode[] CharacterPrompts { get { return characterPrompts; } }
    public DialogueResponseNode[] PlayerResponses { get { return playerResponses; } }
}