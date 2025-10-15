using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AiSims.PlayerInputHandler;
using static UnityEngine.EventSystems.EventTrigger;

namespace AiSims
{
    /// <summary>
    /// Add this component to a GameObject and call the <see cref="IncrementText"/> method
    /// in response to a Unity Event to update a text display to count up with each event.
    /// </summary>
    public class ChangeComplexity : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Text component this behavior uses to display the incremented value.")]
        Text m_Text;

        [Header("Game Complexity")]
        public Complexity complexity;

        [Header("Complexity")]
        public DifficultyLevel difficultyLevel;

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

            if (m_Count == 0)
                difficultyLevel = DifficultyLevel.Easy;
            else if (m_Count == 1)
                difficultyLevel = DifficultyLevel.Challenging;

            if (m_Text != null)
                m_Text.text = m_Count.ToString();

            complexity.globalDifficulty = difficultyLevel;
            complexity.UpdateLlmInstruction();
        }
    }
}
