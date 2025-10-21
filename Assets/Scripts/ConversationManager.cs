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
        public GameObject npcThinkingFeedback;
        public GameObject userTalkingFeedback;

        public Speech2Text speech2Text;
        public bool UserVoiceEnable = false;
        public bool NpcVoiceEnable = false;
        public LLM_Handler questHandler;
        public LLM_Handler companionNPC;
        public float chanceNpcTalking = 0.3f;

        private LLM_Handler currentNPC;

        private Talk talk;
        private MessageDecorator messageDecorator = null;

        private bool isUserTalking = false;
        private bool isNpcTalking = false;
        private string currentMessage;

        private bool isEvaluating = false;
        private LLM_Handler lastNpc = null;

        private Quaternion originalRotation;

        public void Awake()
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

            if (npcThinkingFeedback)
                npcThinkingFeedback.SetActive(false);
            if (userTalkingFeedback)
                userTalkingFeedback.SetActive(false);
        }

        // This function will be called when the NPC finishes talking
        private void OnNpcSpeechFinished()
        {
            Logger.Log(LoggingInfo.DialogueNpc, $"[NPC] NPC talk stop", true);
            NpcConnection npcConnection = currentNPC.GetNpcConnection();
            float chance = Random.value; // float between 0.0 and 1.0
            isNpcTalking = false;

            if (npcConnection != null)
            {
                LLM_Handler nextNpc = npcConnection.RandomHandler;
                if (nextNpc != lastNpc && nextNpc != null && chance < chanceNpcTalking)
                {
                    Debug.Log("Num conversation partner #" + npcConnection.GetNumNpcs() + " | chance = " + chance);
                    Logger.Log(LoggingInfo.LlmProcessing, $"[LlmProcessing] LLM start nextNpc", true);
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
                    Logger.Log(LoggingInfo.LlmProcessing, $"[LlmProcessing] LLM start EvaluateConversation", true);
                    messageDecorator.EvaluateConversation();
                }

                isUserTalking = false;
            }
        }

        public void SetCurrentNPC(NPCToStoryBridge npc)
        {
            currentNPC = npc.llmHandler;
            Logger.Log(LoggingInfo.Scene, $"[NPC] {currentNPC.name}", true);
            messageDecorator.SetEvaluationInstruction(currentNPC.EvaluationString);
        }

        public void SetCurrentNPC(LLM_Handler npc)
        {
            currentNPC = npc;
            messageDecorator.SetEvaluationInstruction(currentNPC.EvaluationString);
        }

        public void TalkUser()
        {
            if (isUserTalking) return;

            if (userTalkingFeedback)
            {
                userTalkingFeedback.SetActive(true);
                PositionMarkerRightOfNPC(currentNPC.npc.transform, userTalkingFeedback.transform);
            }

            Logger.Log(LoggingInfo.DialogueUser, $"[User] User talk start", true);
            if (UserVoiceEnable && currentNPC != null)
            {
                isUserTalking = true;
                speech2Text.Set_LLM_Handler(currentNPC);
                speech2Text.ToggleRecording();
            }
        }

        public void TalkUserFinished()
        {
            if (isNpcTalking) return;

            if (userTalkingFeedback)
                userTalkingFeedback.SetActive(false);

            if (UserVoiceEnable && currentNPC != null)
            {
                isNpcTalking = true;

                if (npcThinkingFeedback)
                {
                    npcThinkingFeedback.SetActive(true);
                    PositionMarkerRightOfNPC(currentNPC.npc.transform, npcThinkingFeedback.transform);
                }

                Logger.Log(LoggingInfo.DialogueUser, "Stop talking", true);
                speech2Text.ToggleRecording();
            }
        }

        public void ProcessMessage(string message)
        {
            if (isUserTalking) return;
            if (message == string.Empty) return;

            if (userTalkingFeedback)
                userTalkingFeedback.SetActive(false);

            isUserTalking = true;

            if (currentNPC != null)
            {
                if (npcThinkingFeedback)
                {
                    npcThinkingFeedback.SetActive(true);
                    PositionMarkerRightOfNPC(currentNPC.npc.transform, npcThinkingFeedback.transform);
                }

                Logger.Log(LoggingInfo.MessageUser, message, true);
                Logger.Log(LoggingInfo.DialogueUser, $"[User] User talk stop", true);
                Logger.Log(LoggingInfo.LlmProcessing, $"[LlmProcessing] LLM start ProcessMessage", true);
                currentNPC.ProcessMessage(message);
            }
        }

        public void TalkNpc(string replyMessage, LLM_Handler npc, string aiName)
        {
            if (!isEvaluating)
                Logger.Log(LoggingInfo.MessageNpc, replyMessage, true);
            else
                Logger.Log(LoggingInfo.MessageNpc, "Evaluation: " + replyMessage, true);
            Logger.Log(LoggingInfo.LlmProcessing, $"[LlmProcessing] LLM stop", true);

            StartCoroutine(TalkNpcCoroutine(replyMessage, npc, aiName));
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
            if (currentNPC.EvaluateConversation() && !isEvaluating)
            {
                //AddMessage(messageDecorator.GetLlmHandler(), currentNPC.GetUserMessage(), MessageTypes.user);
                //AddMessage(messageDecorator.GetLlmHandler(), replyMessage, MessageTypes.assistant);

                messageDecorator.AddChatToHistory(MessageTypes.user.ToString() + ": " + currentNPC.GetUserMessage());
                messageDecorator.AddChatToHistory(MessageTypes.assistant.ToString() + ": " + replyMessage);
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
                if (npcThinkingFeedback)
                    npcThinkingFeedback.SetActive(false);
                Logger.Log(LoggingInfo.DialogueNpc, $"[NPC] NPC talk start", true);

                var voiceHandler = npcHandler.npc.GetComponent<VoiceHandler>();

                // Finally call Text2Speech
                talk.Text2Speech(currentMessage, voiceHandler, npcHandler.GetVoiceName());
            }
            else if (!isEvaluating)
            {
                if (npcThinkingFeedback)
                    npcThinkingFeedback.SetActive(false);
                Logger.Log(LoggingInfo.DialogueNpc, $"[NPC] NPC talk start", true);

                if (messageDecorator != null)
                {
                    messageDecorator.ProcessMessage(replyMessage, aiName);
                }

                // Estimate reading time: characters * factor
                float readingSpeed = 0.05f; // seconds per character (~200 wpm)
                float waitTime = Mathf.Max(1.5f, replyMessage.Length * readingSpeed);
                yield return new WaitForSeconds(waitTime);

                OnNpcSpeechFinished();
            }
            else if (isEvaluating)
            {
                isEvaluating = false;
            }
        }

        public void OrientateNpcToCameraAndStartTalk()
        {
            // Check if stop recording is required
            if (isUserTalking)
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
                if (npcBridge != null && npcBridge.isActiveAndEnabled)
                {
                    Debug.Log("Hit NPC: " + hit.collider.name);

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
            if (isUserTalking)
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
            return isUserTalking;
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