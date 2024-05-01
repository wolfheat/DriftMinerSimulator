using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public class Inputs : MonoBehaviour
{
    public Controls Controls { get; set; }
    public InputAction Actions { get; set; }

    public static Inputs Instance { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("Created Inputs Controller");
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        Controls = new Controls();
        Controls.Enable();
    }
    private void OnEnable()
    {
        SavingUtility.LoadingComplete += LoadingComplete;
    }

    private void LoadingComplete()
    {
        //  Controls.Player.M.performed += SoundMaster.Instance.ToggleMusic;
    }

    private void OnDisable()
    {
        //Controls.Player.M.performed -= SoundMaster.Instance.ToggleMusic;
    }

}
