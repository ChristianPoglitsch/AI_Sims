using UnityEngine;
using UnityEngine.UI;

namespace AiSims
{
    [System.Serializable]
    public class InteractionEntryAdapt
    {
        [Header("Interaction Setup")]
        public int index;
    }

    /// <summary>
    /// Handles quest progression logic and updates UI text.
    /// </summary>
    public class ChangeQuest : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Text component this behavior uses to display the quest index.")]
        private Text m_Text;

        [Header("Game Complexity")]
        public Complexity complexity;

        [Header("Custom Interactions")]
        public InteractionEntryAdapt[] interactionEntries;

        [Header("Clear Chat")]
        public ClearAllLLMChats clearChat;

        [Header("Number of Quests")]
        public int numberOfQuests;

        public Text text
        {
            get => m_Text;
            set => m_Text = value;
        }

        private int m_Count = 0;

        protected void Awake()
        {
            if (m_Text == null)
                Debug.LogWarning("Missing required Text component reference. Assign a Text element in the Inspector.", this);
        }

        protected void Start()
        {
            // Randomize initial quest count
            m_Count = Random.Range(0, numberOfQuests);
            if (m_Text != null)
                m_Text.text = m_Count.ToString();

            Debug.Log($"[Quest] Initialized m_Count randomly to {m_Count}");
            Logger.Log(LoggingInfo.Scene, $"[Quest] {m_Count}", true);

            // Optionally set the initial state in Complexity
            foreach (var entry in interactionEntries)
            {
                if (entry == null) continue;
                complexity.SetLlmQuestByIndex(entry.index, m_Count);
            }
        }

        /// <summary>
        /// Toggles the current quest count (0 - 1) and updates linked LLM entries.
        /// </summary>
        public void Increment()
        {
            m_Count = (m_Count + 1) % numberOfQuests;

            if (m_Text != null)
                m_Text.text = m_Count.ToString();

            // Apply to all configured interaction entries
            foreach (var entry in interactionEntries)
            {
                if (entry == null) continue;

                Debug.Log($"[Quest] Setting LLM index {entry.index} to {m_Count} (toggled)");
                Logger.Log(LoggingInfo.Scene, $"[Quest] {m_Count}", true);
             
                complexity.SetLlmQuestByIndex(entry.index, m_Count);
            }

            if(clearChat != null)
                clearChat.ClearAllChats();
        }
    }
}
