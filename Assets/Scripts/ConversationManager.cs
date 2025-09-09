using LLMUnity;
using ReadyPlayerMe.Core;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public Speech2Text speech2Text;
    public bool VoiceEnable = false;

    private NPCToStoryBridge currentNPC;
    private Talk talk;
    private MessageDecorator messageDecorator = null;

    public void Start()
    {
        talk = GetComponent<Talk>();
        messageDecorator = GetComponent<MessageDecorator>();
    }

    public void SetCurrentNPC(NPCToStoryBridge npc)
    {
        currentNPC = npc;
    }

    public void TalkUser()
    {
        if (VoiceEnable && currentNPC != null && currentNPC.llmHandler != null)
        {
            speech2Text.Set_LLM_Handler(currentNPC.llmHandler);
            if (VoiceEnable)
            {
                speech2Text.ToggleRecording();
            }
        }
        else
        {
            Debug.LogWarning("No NPC or LLM_Handler selected!");
        }
    }

    public void ProcessMessage(string message)
    {
        if (currentNPC && currentNPC.GetHandler() != null)
        {
            currentNPC.GetHandler().ProcessMessage(message);
        }
    }

    public void TalkNpc(string replyMessage, VoiceHandler voiceHandler)
    {
        if (VoiceEnable && talk != null && voiceHandler != null)
        {
            talk.Text2Speech(replyMessage, voiceHandler);
        }

        if (messageDecorator != null)
        {
            messageDecorator.ProcessMessage(replyMessage);
        }
    }
}
