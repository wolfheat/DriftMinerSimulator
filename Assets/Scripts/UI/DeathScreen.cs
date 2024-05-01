using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] UIController UIController;
    [SerializeField] GameObject panel;

    public void ShowScreen()
    {
        Debug.Log("Show Death panel");
        panel.SetActive(true);
    }
    public void CloseClicked()
    {
        Debug.Log("Close clicked");
        UIController.ResetPlayer();
        panel.SetActive(false);
    }
}
