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
        if (messageDecorator != null)
        {
            replyMessage = messageDecorator.FilterMessage(replyMessage);
        }

        if (VoiceEnable && talk != null && voiceHandler != null)
        {
            talk.Text2Speech(replyMessage, voiceHandler);
        }

        if (messageDecorator != null)
        {
            messageDecorator.ProcessMessage(replyMessage);
        }
    }

    public void OrientateNpcToCameraAndStartTalk()
    {
        if (Camera.main == null) return;

        Transform camTransform = Camera.main.transform;

        // Visualize the ray
        Debug.DrawRay(camTransform.position, camTransform.forward * 50f, Color.red, 2f);

        Ray ray = new Ray(camTransform.position, camTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            NPCToStoryBridge npcBridge = hit.collider.GetComponent<NPCToStoryBridge>();
            if (npcBridge != null)
            {
                Debug.Log("Hit NPC: " + hit.collider.name);

                // Make NPC look at player horizontally
                Vector3 lookTarget = camTransform.position;
                lookTarget.y = hit.collider.transform.position.y;
                hit.collider.transform.LookAt(lookTarget);

                Debug.Log("Fire pressed → checking for NPC...");
                SetCurrentNPC(npcBridge);
                TalkUser();
            }
            else
            {
                Debug.Log("Hit object does not have NPCToStoryBridge component.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any NPC.");
        }
    }

}
