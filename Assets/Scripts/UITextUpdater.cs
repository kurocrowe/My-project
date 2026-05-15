using TMPro;
using UnityEngine;
public class UITextUpdater : MonoBehaviour
{
    // Step 2 - add the reference to the text component
    [SerializeField] private TextMeshProUGUI textUI;
    // Step 3 - display the text to the UI
    public void Display(string value)
    {
        textUI.text = value;
    }
}