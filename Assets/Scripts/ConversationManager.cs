using System.Collections;
using ReadyPlayerMe.Core;
using TMPro;
using UnityEngine;

namespace AiSims
{
    public class ConversationManager : MonoBehaviour
    {
        public TMP_Text gameStatusInformation;

        public Speech2Text speech2Text;
        public bool UserVoiceEnable = false;
        public bool NpcVoiceEnable = false;
        public LLM_Handler questHandler;
        public float chanceNpcTalking = 0.3f;

        public LLM_Handler currentNPC;
        private LLM_Handler npcTarget;
        private Talk talk;
        private MessageDecorator messageDecorator = null;

        private bool talking = false;
        private string currentMessage;

        public int maxNpcConversationsUntilEvaluation = 2;
        private int npcConversationsUntilEvaluation;
        public int maxNpcConversations = 1;
        private int npcConversations = 0;

        private readonly string userCanTalk = "User can talk.";
        private readonly string npcTalking = "NPC is thinking.";

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

            messageDecorator.SetLlmHandler(questHandler);
        }

        // This function will be called when the NPC finishes talking
        private void OnNpcSpeechFinished()
        {
            npcConversations = Mathf.Max(-1, npcConversations - 1);
            npcConversationsUntilEvaluation = Mathf.Max(-1, npcConversationsUntilEvaluation - 1);

            NpcConnection otherNpc = currentNPC.GetNpcConnection();
            float chance = Random.value; // float between 0.0 and 1.0

            if (otherNpc != null && otherNpc.RandomHandler != null && chance < chanceNpcTalking)
            {
                gameStatusInformation.text = string.Empty;

                Debug.Log("Num conversation partner #" + otherNpc.GetNumNpcs() + " | chance = " + chance);
                currentNPC = otherNpc.RandomHandler;
                if (currentNPC)
                {
                    currentNPC.ProcessMessage(currentMessage);
                }
            }
            else
            {
                Debug.Log("NPC 1:1 conversation. | chance = " + chance);
            }

            if (npcConversationsUntilEvaluation == 0 && currentNPC.EvaluateConversation())
            {
                messageDecorator.EvaluateConversation();
            }

            // Now it is the user's turn
            if (npcConversations == 0)
            {
                npcConversations = maxNpcConversations;
                if (npcTarget != null)
                {
                    currentNPC = npcTarget;
                }
            }

            gameStatusInformation.text = userCanTalk;
            talking = false;
            Debug.Log("NPC finished speaking. #num conversations left is " + npcConversations + " ; #num conversations until evaluation " + npcConversationsUntilEvaluation);
        }

        public void SetCurrentNPC(NPCToStoryBridge npc)
        {
            currentNPC = npc.llmHandler;
            npcTarget = currentNPC;
            npcConversationsUntilEvaluation = maxNpcConversationsUntilEvaluation;
            npcConversations = maxNpcConversations;

            if (currentNPC && currentNPC.GetNpcConnection() != null)
            {
                maxNpcConversations = currentNPC.GetNpcConnection().GetNumNpcs() + 1;
                npcConversations = maxNpcConversations;
                Debug.Log("Number of NPC conversations set to " + npcConversations);
            }

            messageDecorator.SetEvaluationInstruction(currentNPC.EvaluationString);
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
            if (gameStatusInformation.text == string.Empty) return;
            if (talking) return;
            talking = true;

            if (gameStatusInformation)
                gameStatusInformation.text = npcTalking;

            if (currentNPC != null)
            {
                currentNPC.ProcessMessage(message);
            }
        }

        public void TalkNpc(string replyMessage, VoiceHandler voiceHandler, bool addToHist, string aiName)
        {
            StartCoroutine(TalkNpcCoroutine(replyMessage, voiceHandler, addToHist, aiName));
        }

        private IEnumerator TalkNpcCoroutine(string replyMessage, VoiceHandler voiceHandler, bool addToHist, string aiName)
        {
            if (addToHist)
            {
                messageDecorator.AddMessage(currentNPC.GetUserMessage(), MessageTypes.user);
                messageDecorator.AddMessage(replyMessage, MessageTypes.assistant);
            }

            currentMessage = replyMessage;

            if (NpcVoiceEnable && talk != null && voiceHandler != null)
            {
                talk.Text2Speech(replyMessage, voiceHandler, currentNPC.GetVoiceName());
            }
            else
            {
                gameStatusInformation.text = string.Empty;

                if (messageDecorator != null)
                {
                    messageDecorator.ProcessMessage(replyMessage, aiName);
                }

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

            // Use mouse position instead of always forward
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.green, 2f);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 6f))
            {
                NPCToStoryBridge npcBridge = hit.collider.GetComponent<NPCToStoryBridge>();
                if (npcBridge != null)
                {
                    Debug.Log("Hit NPC: " + hit.collider.name);
                    gameStatusInformation.text = userCanTalk;

                    // Make NPC look at player horizontally
                    Vector3 lookTarget = Camera.main.transform.position;
                    lookTarget.y = hit.collider.transform.position.y;
                    hit.collider.transform.LookAt(lookTarget);
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

        public void OrientateNpcToCameraAndStartTalkNoRayCast(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject provided to OrientateNpcToCameraAndStartTalkNoRayCast.");
                return;
            }

            // Get the NPCToStoryBridge component from the object
            NPCToStoryBridge npcBridge = selectedObject.GetComponent<NPCToStoryBridge>();
            if (npcBridge == null)
            {
                Debug.LogWarning("The selected GameObject does not have an NPCToStoryBridge component.");
                return;
            }

            Debug.Log("Selected NPC: " + npcBridge.name);
            gameStatusInformation.text = userCanTalk;

            // Make NPC look at the player horizontally
            if (Camera.main != null)
            {
                Vector3 lookTarget = Camera.main.transform.position;
                lookTarget.y = npcBridge.transform.position.y;
                npcBridge.transform.LookAt(lookTarget);
            }

            SetCurrentNPC(npcBridge);
            TalkUser();
        }
    }
}