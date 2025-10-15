using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AiSims.PlayerInputHandler;

namespace AiSims
{
    // Updated InteractionEntry
    [System.Serializable]
    public class InteractionEntryAdapt
    {
        [Header("Interaction Setup")]
        public int index;
    }

    /// <summary>
    /// Add this component to a GameObject and call the <see cref="IncrementText"/> method
    /// in response to a Unity Event to update a text display to count up with each event.
    /// </summary>
    public class ChangeQuest : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Text component this behavior uses to display the incremented value.")]
        Text m_Text;

        [Header("Game Complexity")]
        public Complexity complexity;

        [Header("Custom Interactions")]
        public InteractionEntryAdapt[] interactionEntries;

        /// <summary>
        /// The Text component this behavior uses to display the incremented value.
        /// </summary>
        public Text text
        {
            get => m_Text;
            set => m_Text = value;
        }

        int m_Count = 0;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (m_Text == null)
                Debug.LogWarning("Missing required Text component reference. Use the Inspector window to assign which Text component to increment.", this);
        }

        /// <summary>
        /// Increment the string message of the Text component.
        /// </summary>
        public void IncrementText()
        {
            m_Count += 1;
            if (m_Count > 1) m_Count = 0;
            if (m_Text != null)
                m_Text.text = m_Count.ToString();

            foreach (var entry in interactionEntries)
            {
                if (entry == null) continue;

                Debug.Log($"[ChangeScene] Setting LLM index {entry.index} to {m_Count} (triggered by count {m_Count})");
                complexity.SetLlmQuestByIndex(entry.index, m_Count);
            }
        }
    }
}
