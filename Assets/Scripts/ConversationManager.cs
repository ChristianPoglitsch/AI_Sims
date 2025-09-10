using LLMUnity;
using ReadyPlayerMe.Core;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public Speech2Text speech2Text;
    public bool UserVoiceEnable = false;
    public bool NpcVoiceEnable = false;

    private NPCToStoryBridge currentNPC;
    private Talk talk;
    private MessageDecorator messageDecorator = null;

    private bool talking = false;

    public void Start()
    {
        talk = GetComponent<Talk>();
        messageDecorator = GetComponent<MessageDecorator>();

        // Subscribe to speech finished event
        if (talk != null)
        {
            talk.OnSpeechFinished += OnNpcSpeechFinished;
        }
    }

    // This function will be called when the NPC finishes talking
    private void OnNpcSpeechFinished()
    {
        talking = false;

        Debug.Log("NPC finished speaking!");
        // You can trigger next actions here, e.g. enable user input
    }

    public void SetCurrentNPC(NPCToStoryBridge npc)
    {
        currentNPC = npc;
    }

    public void TalkUser()
    {
        if (talking) return;

        if (UserVoiceEnable && currentNPC != null && currentNPC.llmHandler != null)
        {
            talking = true;

            speech2Text.Set_LLM_Handler(currentNPC.llmHandler);
            speech2Text.ToggleRecording();
        }
    }

    public void ProcessMessage(string message)
    {
        if (talking) return;
        talking = true;

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

        if (NpcVoiceEnable && talk != null && voiceHandler != null)
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
