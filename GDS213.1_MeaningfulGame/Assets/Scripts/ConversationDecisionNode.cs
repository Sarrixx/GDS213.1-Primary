using UnityEngine;

[CreateAssetMenu(fileName = "New Decision Node", menuName = "Conversation Nodes/Decision Node")]
public class ConversationDecisionNode : ConversationNode
{
    [SerializeField] private DecisionNode[] decisions;

    public DecisionNode[] Decisions { get { return decisions; } }
}