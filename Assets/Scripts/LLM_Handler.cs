using LLMUnity;
using ReadyPlayerMe.Core;
using System.Xml.Linq;
using UnityEngine;

namespace AiSims
{
    public class LLM_Handler : MonoBehaviour
    {
        public VoiceHandler voiceHandler;
        public ConversationManager conversationManager;
        public string voice = "alloy";
        public bool enableEvaluation = false;

        private NpcConnection connection;
        private LLMCharacter llmCharacter;
        private string replyMessage;
        private string userMessage;

        bool addToHistory = false;

        [TextArea(5, 10), Chat] public string EvaluationString = string.Empty;

        private void Start()
        {
            llmCharacter = GetComponent<LLMCharacter>();
            connection = GetComponent<NpcConnection>();
        }

        public bool EvaluateConversation()
        {
            return enableEvaluation;
        }

        public string GetVoiceName()
        {
            return voice;
        }

        public string GetUserMessage()
        {
            return userMessage;
        }

        public NpcConnection GetNpcConnection()
        {
            return connection;
        }

        public LLMCharacter GetLlm()
        {
            return llmCharacter;
        }

        void HandleReply(string reply)
        {
            replyMessage = reply;
        }

        void ReplyCompleted()
        {
            Debug.Log(llmCharacter.AIName + ": " + replyMessage);
            conversationManager.TalkNpc(replyMessage, voiceHandler, addToHistory, llmCharacter.AIName);
        }

        public void ProcessMessage(string message, bool addToHist = true)
        {
            Debug.Log(message);
            userMessage = message;
            addToHistory = addToHist;
            _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted, addToHistory);
        }

        public void AddMessage(string message, string userName)
        {
            llmCharacter.AddMessage(userName, message);
        }
    }
}
