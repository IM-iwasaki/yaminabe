using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CursorUI : MonoBehaviour
{
    [SerializeField]private Image cursorUI;
    [SerializeField] private VirtualMouseInput mouseInput;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        ToggleCursor(true);
    }

    public void ToggleCursor(bool _isOpen)
    {
        if (Gamepad.current == null) return;
        if (_isOpen)
        {
            cursorUI.gameObject.SetActive(true);
            mouseInput.enabled = true;
        }

        else
        {
            cursorUI.gameObject.SetActive(false);
            mouseInput.enabled = false;
        }
            
    }
    public void DestoryObject()
    {
        Destroy(gameObject);
    }
}
