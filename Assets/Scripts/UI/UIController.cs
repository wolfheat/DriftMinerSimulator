using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject middleCursor;


    // OLD
    [SerializeField] TextMeshProUGUI speed;
    [SerializeField] TextMeshProUGUI tilt;
    [SerializeField] TextMeshProUGUI playerTilt;
    [SerializeField] TextMeshProUGUI oxygenText;
    [SerializeField] Volume volume;
    [SerializeField] DeathScreen deathScreen;
    [SerializeField] WinScreen winScreen;
    [SerializeField] GameObject debugHelpText;
    [SerializeField] PauseController pauseScreen;
    [SerializeField] GameObject tempHair;

    //[SerializeField] InventoryUI inventoryUI;
    [SerializeField] ToggleMenu inventoryToggle;
    //[SerializeField] CraftingUI craftingUI;
    [SerializeField] ToggleMenu craftingToggle;

    [SerializeField] HUDIcons hudIcons;
    [SerializeField] Image image;
    //[SerializeField] Player player;
    [SerializeField] PlayerStats playerStats;


    public static UIController Instance;
    public static bool InventoryActive { get { return Instance.inventoryToggle.IsActive; } }
    public static bool CraftingActive { get { return Instance.craftingToggle.IsActive; }}


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void ShowCursor(bool show = true) => middleCursor?.gameObject.SetActive(show);


    public void OnEnable()
    {        
        Inputs.Instance.Controls.Player.Tab.started += Toggle;
        Inputs.Instance.Controls.Player.Esc.started += Pause;

        Pause(false);
    }
    
    public void OnDisable()
    {
        
        Inputs.Instance.Controls.Player.Tab.started -= Toggle;
        Inputs.Instance.Controls.Player.Esc.started -= Pause;
    }


    public void Pause(InputAction.CallbackContext context)
    {
        // Player can not toggle pause when dead
        if (playerStats.IsDead) return;

        bool doPause = GameState.state == GameStates.Running;
        Pause(doPause);
        pauseScreen.SetActive(doPause);
    }

    public void Pause(bool pause = true)
    {
        GameState.state = pause?GameStates.Paused:GameStates.Running;
        Debug.Log("Gamestate set to "+ GameState.state);
        Time.timeScale = pause?0f:1f;
    }

    public void Toggle(InputAction.CallbackContext context)
    {
        // Disable interact when inventory
        if (GameState.IsPaused)
        {
            Debug.Log("Cannot toggle inventory because game is paused");
            return;
        }

        // Request To toggle inventory
        if (craftingToggle.IsActive)
        {
            Debug.Log("Trying to Toggle inventory but crafting is active");
            //craftingUI.Reset();
            craftingToggle.HideMenu();  
        }
        else
        {
            //inventoryToggle.Toggle();
        }
    }

    public void SetSpeed(Vector3 s)
    {
        speed.text = "Speed: ("+s.x.ToString("F")+","+s.y.ToString("F") + ","+s.z.ToString("F") + ")("+s.magnitude.ToString("F") + ")";
    }
    public void HideHUDIcon()
    {
        hudIcons.Disable();
    }
    public void HideTempHair()
    {
        tempHair.SetActive(false);
    }

    public void InventoryChanged()
    {
       // inventoryUI.UpdateInventory();
        
    }

    public void SetTilt(float x)
    {
        tilt.text = "Tilt: (" + x + ")";
    }

    public void SetPlayerTilt(Vector3 a)
    {
        playerTilt.text = "PlayerTilt: ("+a.x+","+a.y+","+a.z+")";
    }

    public void SetOxygen(float oxygen, int maxOxygen)
    {
        oxygenText.text = "Oxygen: (" + oxygen.ToString("F") + "/" + maxOxygen + ")";
    }
    public void UpdateScreenDarkening(float percent)
    {
        image.color = new Color() { a = percent };
        volume.weight = percent;
    }
    public void ShowDeathScreen()
    {
        Pause();
        playerStats.SetToDead();
        deathScreen.ShowScreen();
    }
    public void ShowWinScreen()
    {
        Debug.Log("Player WON");
        GameState.state = GameStates.Paused;
        winScreen.ShowScreen();
    }
    public void ResetPlayer(bool keepStats = false)
    {
        //player.ResetPlayer(keepStats);
        Pause(false);
    }

}
