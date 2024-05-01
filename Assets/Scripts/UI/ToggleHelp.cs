using UnityEngine;

public class ToggleHelp : MonoBehaviour
{
    [SerializeField] private GameObject panelToToggle;
    
    public void Toggle()
    {
        Debug.Log("Toggle Help");
        panelToToggle.SetActive(!panelToToggle.gameObject.activeSelf);
    }
}
