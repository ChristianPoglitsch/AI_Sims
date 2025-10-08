using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using ReadyPlayerMe.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows;

namespace AiSims
{
    public class ConversationManager : MonoBehaviour
    {
        public TMP_Text gameStatusInformation;
        public GameObject npcThinking;

        public Speech2Text speech2Text;
        public bool UserVoiceEnable = false;
        public bool NpcVoiceEnable = false;
        public LLM_Handler questHandler;
        public LLM_Handler companionNPC;
        public float chanceNpcTalking = 0.3f;

        private LLM_Handler currentNPC;       

        private Talk talk;
        private MessageDecorator messageDecorator = null;

        private bool talking = false;
        private bool isNpcTalking = false;
        private string currentMessage;

        private bool isEvaluating = false;
        private LLM_Handler lastNpc = null;

        private readonly string userCanTalk = "User can talk ... ";
        private readonly string userIsTalking = "User is talking ... ";
        private readonly string npcTalking = "NPC is thinking ...";        

        private Quaternion originalRotation;

        public void Start()
        {
            talk = GetComponent<Talk>();
            messageDecorator = GetComponent<MessageDecorator>();

            // Subscribe to speech finished event
            if (talk != null)
            {
                talk.OnSpeechFinished += OnNpcSpeechFinished;
            }

            if (questHandler)
            {
                messageDecorator.SetLlmHandler(questHandler);
            }
            Logger.Log(LoggingInfo.Scene, "Start Scene", true);

            if(npcThinking)
                npcThinking.SetActive(false);
        }

        // This function will be called when the NPC finishes talking
        private void OnNpcSpeechFinished()
        {
            NpcConnection npcConnection = currentNPC.GetNpcConnection();
            float chance = Random.value; // float between 0.0 and 1.0
            isNpcTalking = false;

            if (npcConnection != null)
            {
                LLM_Handler nextNpc = npcConnection.RandomHandler;
                if (nextNpc != lastNpc && nextNpc != null && chance < chanceNpcTalking)
                {
                    gameStatusInformation.text = string.Empty;

                    Debug.Log("Num conversation partner #" + npcConnection.GetNumNpcs() + " | chance = " + chance);
                    nextNpc.ProcessMessage(currentMessage);
                    lastNpc = nextNpc;
                }
            }
            else
            {
                Debug.Log("NPC 1:1 conversation. | chance = " + chance);
            }

            if (!isNpcTalking)
            {
                if (currentNPC.EvaluateConversation())
                {
                    isEvaluating = true;
                    messageDecorator.EvaluateConversation();
                }

                gameStatusInformation.text = userCanTalk;
                talking = false;
            }
        }

        public void SetCurrentNPC(NPCToStoryBridge npc)
        {
            currentNPC = npc.llmHandler;
            messageDecorator.SetEvaluationInstruction(currentNPC.EvaluationString);
        }

        public void SetCurrentNPC(LLM_Handler npc)
        {
            currentNPC = npc;
            messageDecorator.SetEvaluationInstruction(currentNPC.EvaluationString);
        }

        public void TalkUser()
        {
            if (talking) return;
            gameStatusInformation.text = userIsTalking;

            if (UserVoiceEnable && currentNPC != null)
            {
                talking = true;
                speech2Text.Set_LLM_Handler(currentNPC);
                speech2Text.ToggleRecording();
            }
        }

        public void TalkUserFinished()
        {
            if (isNpcTalking) return;

            if (UserVoiceEnable && currentNPC != null)
            {
                isNpcTalking = true;
                gameStatusInformation.text = npcTalking;

                if (npcThinking)
                {
                    npcThinking.SetActive(true);
                    PositionMarkerRightOfNPC(currentNPC.npc.transform, npcThinking.transform);
                }

                speech2Text.ToggleRecording();
            }
        }

        public void ProcessMessage(string message)
        {
            if (talking) return;
            if (message == string.Empty) return;

            talking = true;
            gameStatusInformation.text = npcTalking;

            if (currentNPC != null)
            {
                if (npcThinking)
                {
                    npcThinking.SetActive(true);
                    PositionMarkerRightOfNPC(currentNPC.npc.transform, npcThinking.transform);
                }

                Logger.Log(LoggingInfo.DialogueUser, message, true);

                currentNPC.ProcessMessage(message);
            }
        }

        public void TalkNpc(string replyMessage, LLM_Handler npc, string aiName)
        {
            Logger.Log(LoggingInfo.DialoagNpc, replyMessage, true);

            StartCoroutine(TalkNpcCoroutine(replyMessage, npc,  aiName));
        }

        public void AddMessage(LLM_Handler handler, string message, MessageTypes type)
        {
            if (handler == null || handler.GetLlm() == null)
            {
                Debug.Log("LLM Handler for evaluating quests is not assigned.");
                return;
            }

            // Convert enum to string role
            string role = type.ToString(); // "system", "assistant", "user"

            // Call LLM with role and message
            handler.GetLlm().AddMessage(role, role + ": " + message);
        }

        private IEnumerator TalkNpcCoroutine(string replyMessage, LLM_Handler npcHandler, string aiName)
        {
            if (currentNPC.EvaluateConversation())
            {
                AddMessage(messageDecorator.GetLlmHandler(), currentNPC.GetUserMessage(), MessageTypes.user);
                AddMessage(messageDecorator.GetLlmHandler(), replyMessage, MessageTypes.assistant);
            }

            if (companionNPC && currentNPC != companionNPC)
            {
                AddMessage(companionNPC, currentNPC.GetUserMessage(), MessageTypes.user);
                AddMessage(companionNPC, replyMessage, MessageTypes.assistant);
            }

            NpcConnection otherNpc = currentNPC.GetNpcConnection();
            if (otherNpc != null)
            {
                var allHandler = otherNpc.GetAllHandler();
                foreach (var handler in allHandler)
                {
                    handler.AddMessage(MessageTypes.user.ToString() + ' ' + currentNPC.GetUserMessage(), MessageTypes.user.ToString());
                    handler.AddMessage(aiName + ' ' + replyMessage, MessageTypes.user.ToString());
                }
            }

            currentMessage = replyMessage;

            if (NpcVoiceEnable && talk != null && npcHandler.npc != null)
            {
                gameStatusInformation.text = string.Empty;
                if (npcThinking)
                    npcThinking.SetActive(false);

                var voiceHandler = npcHandler.npc.GetComponent<VoiceHandler>();

                // Finally call Text2Speech
                talk.Text2Speech(currentMessage, voiceHandler, npcHandler.GetVoiceName());
            }
            else if(!isEvaluating)
            {
                gameStatusInformation.text = string.Empty;
                if (npcThinking)
                    npcThinking.SetActive(false);

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
            else if(isEvaluating)
            {
                isEvaluating = false;
            }
        }

        public void OrientateNpcToCameraAndStartTalk()
        {
            // Check if stop recording is required
            if (talking)
            {
                return;
            }

            if (Camera.main == null) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("Pointer over UI - not raycasting NPCs.");
                return;
            }

            // Use mouse position instead of always forward
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
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
            }
        }

        public void OrientateNpcToCameraAndStartTalkNoRayCast(GameObject selectedObject)
        {
            // Check if stop recording is required
            if (talking)
            {
                return;
            }

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

            //Debug.Log("Selected NPC: " + npcBridge.name);
            originalRotation = npcBridge.transform.rotation;

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

        public void RestoreNpcOrientation(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                Debug.LogWarning("No NPC stored to restore orientation.");
                return;
            }

            selectedObject.transform.rotation = originalRotation;
            Debug.Log("Restored orientation for NPC: " + selectedObject.name);
        }

        public bool Talking()
        {
            return talking;
        }

        /// <summary>
        /// Positions the target transform (e.g., npcThinking) one meter to the right
        /// of the specified NPC, based on the NPC's current world orientation.
        /// Keeps the object independent (not parented) and updates immediately.
        /// </summary>
        /// <param name="npc">The NPC whose position and rotation define the placement.</param>
        /// <param name="marker">The independent transform to position (e.g., npcThinking).</param>
        /// <param name="horizontalDistance">Horizontal offset in meters to the right of the NPC (default 0.3).</param>
        /// <param name="verticalOffset">Vertical offset in meters above the NPC (default 0.1).</param>
        /// <param name="faceCamera">If true, rotates the marker to face the main camera instantly.</param>
        public void PositionMarkerRightOfNPC(Transform npc, Transform marker,
            float horizontalDistance = 0.4f, float verticalOffset = 1.7f, bool faceCamera = true)
        {
            if (npc == null || marker == null)
                return;

            // Calculate right-hand direction based on NPC orientation
            Vector3 rightDir = npc.right;

            // Compute target position: right side + vertical offset
            Vector3 targetPos = npc.position + rightDir * horizontalDistance;
            targetPos.y = npc.position.y + verticalOffset;

            // Instantly set position (no interpolation)
            marker.position = targetPos;

            // Update rotation instantly
            if (faceCamera && Camera.main != null)
            {
                // Make the marker face the camera directly
                marker.LookAt(Camera.main.transform);

                // Optionally, keep upright (prevent tilting)
                Vector3 euler = marker.eulerAngles;
                euler.x = 0f;
                euler.z = 0f;
                marker.eulerAngles = euler;
            }
            else
            {
                // Align rotation with the NPC
                marker.rotation = npc.rotation;
            }
        }

    }
}