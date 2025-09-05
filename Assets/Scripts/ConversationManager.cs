using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public LLM_Handler llmPersona;
    public Speech2Text speech2Text;

    public void Talk()
    {
        speech2Text.Set_LLM_Handler(llmPersona);
        speech2Text.ToggleRecording();
    }
}
