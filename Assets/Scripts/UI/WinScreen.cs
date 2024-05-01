using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] UIController UIController;
    [SerializeField] GameObject panel;

    public void ShowScreen()
    {
        Debug.Log("Show Win panel");
        panel.SetActive(true);
    }
    public void CloseClicked()
    {
        Debug.Log("Close clicked");
        UIController.ResetPlayer();
        panel.SetActive(false);
    }
}
