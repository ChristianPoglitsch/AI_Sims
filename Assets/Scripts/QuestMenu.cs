using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;   

    [SerializeField] 
    private InputActionReference toggleAction;

    [SerializeField]
    private TMP_Text text;

    void OnEnable()
    {
        if (menu)
        {
            menu.SetActive(false);
        }

        if (toggleAction != null)
        {
            toggleAction.action.performed += OnToggle;
            toggleAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnToggle;
            toggleAction.action.Disable();
        }
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        if (!menu)
        {
            return;
        }

        menu.SetActive(!menu.activeSelf);
    }

    public void SetText(string newText)
    {
        if (text)
        {
            text.text = newText;
        }
    }
}
