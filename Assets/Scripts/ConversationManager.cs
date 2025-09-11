using System.Collections;
using ReadyPlayerMe.Core;
using TMPro;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public TMP_Text gameStatusInformation;

    public Speech2Text speech2Text;
    public bool UserVoiceEnable = false;
    public bool NpcVoiceEnable = false;
    public LLM_Handler questHandler;

    private LLM_Handler currentNPC;
    private LLM_Handler npcTarget;
    private Talk talk;
    private MessageDecorator messageDecorator = null;

    private bool talking = false;
    private string currentMessage;

    public int maxNpcConversationsUntilEvaluation = 2;
    private int npcConversationsUntilEvaluation;
    public int maxNpcConversations = 1;
    private int npcConversations = 0;

    private string userCanTalk = "User can talk.";
    private string npcTalking = "NPC is thinking.";

    public void Start()
    {
        talk = GetComponent<Talk>();
        messageDecorator = GetComponent<MessageDecorator>();

        // Subscribe to speech finished event
        if (talk != null)
        {
            talk.OnSpeechFinished += OnNpcSpeechFinished;
        }

        npcConversations = maxNpcConversations;
        npcConversationsUntilEvaluation = maxNpcConversationsUntilEvaluation;
    }

    // This function will be called when the NPC finishes talking
    private void OnNpcSpeechFinished()
    {
        npcConversations--;        
        talking = false;
        gameStatusInformation.text = userCanTalk;

        Debug.Log("NPC finished speaking. #num conversations left is " + npcConversations);
        // You can trigger next actions here, e.g. enable user input

        NpcConnection otherNpc = currentNPC.GetNpcConnection();
        if (npcConversations > 0 && otherNpc != null)
        {
            Debug.Log("Num conversation partner #" + otherNpc.GetNumNpcs());
            currentNPC = otherNpc.RandomHandler;
            if (currentNPC)
            {
                currentNPC.ProcessMessage(currentMessage);
            }
        }
        else
        {
            Debug.Log("NPC 1:1 conversation.");
        }

        if(npcConversationsUntilEvaluation == 0)
        {
            messageDecorator.SetLlmHandler(questHandler);
            messageDecorator.EvaluateConversation();
        }

        // Now it is the user's turn
        if(npcConversations < 0)
        {
            npcConversationsUntilEvaluation--;
            npcConversations = maxNpcConversations;
            currentNPC = npcTarget;
        }
    }

    public void SetCurrentNPC(NPCToStoryBridge npc)
    {        
        currentNPC = npc.llmHandler;
        npcTarget = currentNPC;
        npcConversationsUntilEvaluation = maxNpcConversationsUntilEvaluation;

        if (currentNPC && currentNPC.GetNpcConnection() != null)
        {
            maxNpcConversations = currentNPC.GetNpcConnection().GetNumNpcs();
            npcConversations = maxNpcConversations;

            npcConversations = currentNPC.GetNpcConnection().GetNumNpcs();
            Debug.Log("Number of NPC conversations set to " + npcConversations);
        }
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

        gameStatusInformation.text = npcTalking;
        if (currentNPC != null)
        {
            currentNPC.ProcessMessage(message);
        }
    }

    public void TalkNpc(string replyMessage, VoiceHandler voiceHandler)
    {
        StartCoroutine(TalkNpcCoroutine(replyMessage, voiceHandler));
    }

    private IEnumerator TalkNpcCoroutine(string replyMessage, VoiceHandler voiceHandler)
    {
        messageDecorator.AddMessage(currentNPC.GetUserMessage());
        //messageDecorator.AddMessage(replyMessage);

        currentMessage = replyMessage;

        if (NpcVoiceEnable && talk != null && voiceHandler != null)
        {
            talk.Text2Speech(replyMessage, voiceHandler, currentNPC.GetVoiceName());
        }
        else
        {
            if (messageDecorator != null)
            {
                messageDecorator.ProcessMessage(replyMessage);
            }

            gameStatusInformation.text = string.Empty;

            // 🕒 Estimate reading time: characters * factor
            float readingSpeed = 0.05f; // seconds per character (~200 wpm)
            float waitTime = Mathf.Max(1.5f, replyMessage.Length * readingSpeed);
            yield return new WaitForSeconds(waitTime);

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
                gameStatusInformation.text = userCanTalk;

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
