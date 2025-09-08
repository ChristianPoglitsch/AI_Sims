using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    private NPCToStoryBridge currentNPC;
    public Speech2Text speech2Text;

    public void SetCurrentNPC(NPCToStoryBridge npc)
    {
        currentNPC = npc;
    }

    public void Talk()
    {
        if (currentNPC != null && currentNPC.llmHandler != null)
        {
            speech2Text.Set_LLM_Handler(currentNPC.llmHandler);
            //speech2Text.ToggleRecording();
        }
        else
        {
            Debug.LogWarning("No NPC or LLM_Handler selected!");
        }
    }
}
