using System.Collections.Generic;
using NightStand.Data;

namespace NightStand.Dialogue
{
    /// <summary>
    /// โหนดในโครงสร้างต้นไม้บทสนทนา (N-ary Tree Node) สำหรับการซักถาม NPC แบบแตกแขนง
    /// </summary>
    public class DialogueNode
    {
        public string NodeId { get; }
        public string PromptLabel { get; }
        public string ResponseLine { get; }

        // [Data Structure: List<DialogueNode> - Children Adjacency List in N-ary Tree]
        // Stores child branches for follow-up interrogation questions.
        // Time Complexity: AddChild O(1) amortized, GetChild(index) O(1)
        public List<DialogueNode> Children { get; } = new List<DialogueNode>();

        public DialogueNode(string nodeId, string promptLabel, string responseLine)
        {
            NodeId = nodeId;
            PromptLabel = promptLabel;
            ResponseLine = responseLine;
        }

        public DialogueNode AddChild(DialogueNode child)
        {
            if (child != null)
            {
                Children.Add(child);
            }

            return child;
        }

        // [Operation: O(1) Direct Child Branch Access]
        public DialogueNode GetChild(int index)
        {
            return index >= 0 && index < Children.Count ? Children[index] : null;
        }
    }

    /// <summary>
    /// [Data Structure: N-ary Tree (DialogueTree) - Branching NPC Interrogation Tree]
    /// Organizes NPC responses hierarchically from Root (Greeting) to Question Branches and Escalation Subtrees.
    /// Time Complexity: Tree Construction O(k) where k = number of dialogue nodes, Branch Traversal O(1) per step
    /// </summary>
    public class DialogueTree
    {
        public DialogueNode Root { get; }

        public DialogueTree(DialogueNode root)
        {
            Root = root;
        }

        public static DialogueTree BuildFromNPC(NPCDataSO npc)
        {
            if (npc == null)
            {
                return new DialogueTree(new DialogueNode("root", "Greeting", "..."));
            }

            DialogueNode root = new DialogueNode("root", "Greeting", npc.GetGreeting());

            // Depth 1 Branches: 0 = Identity, 1 = Purpose, 2 = Suspicion (Level 1)
            root.AddChild(new DialogueNode("identity", "Ask Name / House", npc.GetIdentityResponse()));
            root.AddChild(new DialogueNode("purpose", "Ask Purpose", npc.GetPurposeResponse()));

            // Depth 1 -> Depth 2 -> Depth 3 Escalating Interrogation Subtree
            DialogueNode susp1 = root.AddChild(new DialogueNode("suspicious_1", "Press Suspicion (1)", npc.GetSuspiciousResponse(1)));
            DialogueNode susp2 = susp1.AddChild(new DialogueNode("suspicious_2", "Press Suspicion (2)", npc.GetSuspiciousResponse(2)));
            susp2.AddChild(new DialogueNode("suspicious_3", "Press Suspicion (3)", npc.GetSuspiciousResponse(3)));

            return new DialogueTree(root);
        }
    }
}
