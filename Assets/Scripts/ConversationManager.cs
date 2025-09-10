using LLMUnity;
using ReadyPlayerMe.Core;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public Speech2Text speech2Text;
    public bool UserVoiceEnable = false;
    public bool NpcVoiceEnable = false;

    private LLM_Handler currentNPC;
    private Talk talk;
    private MessageDecorator messageDecorator = null;

    private bool talking = false;
    private string currentMessage;

    public int maxNpcConversations = 1;

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

        NpcConnection otherNpc = currentNPC.GetNpcConnection();
        if (maxNpcConversations > 0 && otherNpc != null)
        {
            maxNpcConversations--;

            Debug.Log("Num conversation partner #" + otherNpc.GetNumNpcs());
            SetCurrentNPC(otherNpc.RandomHandler);
            if (currentNPC)
            {
                currentNPC.ProcessMessage(currentMessage);
            }
        }
        else
        {
            Debug.Log("NPC 1:1 conversation.");
        }
    }

    public void SetCurrentNPC(LLM_Handler npc)
    {
        currentNPC = npc;
    }

    public void SetCurrentNPC(NPCToStoryBridge npc)
    {
        currentNPC = npc.llmHandler;
    }

    public void TalkUser()
    {
        if (talking) return;

        if (UserVoiceEnable && currentNPC != null && currentNPC != null)
        {
            talking = true;

            speech2Text.Set_LLM_Handler(currentNPC);
            speech2Text.ToggleRecording();
        }
    }

    public void ProcessMessage(string message)
    {
        if (talking) return;
        talking = true;

        if (currentNPC != null)
        {
            currentNPC.ProcessMessage(message);
        }
    }

    public void TalkNpc(string replyMessage, VoiceHandler voiceHandler)
    {
        if (messageDecorator != null)
        {
            replyMessage = messageDecorator.FilterMessage(replyMessage);
        }

        currentMessage = replyMessage;

        if (NpcVoiceEnable && talk != null && voiceHandler != null)
        {
            talk.Text2Speech(replyMessage, voiceHandler, currentNPC.GetVoiceName());
        }

        if (messageDecorator != null)
        {
            messageDecorator.ProcessMessage(replyMessage);
        }

        if(!NpcVoiceEnable)
        {
            OnNpcSpeechFinished();
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
