using TMPro;
using UnityEngine;
using Wolfheat.StartMenu;

public class HUDMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI negativeText;
    [SerializeField] private GameObject negative;
    [SerializeField] private TextMeshProUGUI positiveText;
    [SerializeField] private GameObject positive;
    [SerializeField] private GameObject textObject;
    [SerializeField] private Animator animator;


    public static HUDMessage Instance;
    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }


    public void ShowMessage(string message, bool isNegative = true,SoundName sound = SoundName.HUDError)
    {
        positiveText.text = isNegative?"":message;
        negativeText.text = isNegative ? message:"";
        animator.Play("ShowMessage");
        SoundMaster.Instance.PlaySound(sound);
    }
}
