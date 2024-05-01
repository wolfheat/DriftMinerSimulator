using System;
using System.Collections;
using UnityEngine;
using Wolfheat.StartMenu;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] UIController uiController;
    [SerializeField] Rigidbody rb;
    //[SerializeField] EquipedGrid equiped;
    [SerializeField] InfoHeader infoHeader;
    private int health = 100;
    private float oxygen = 11;
    private int speed = 2;
    
    private float noOxygenSurvival = 8f;
    private const float NoOxygenSurvivalMax = 8f;

    private int maxHealth = 100;
    private int maxOxygen = 70;
    private int maxSpeed = 2;
    public int MaxSpeed { get { return maxSpeed; }}
    public bool AtMaxHealth { get { return health==maxHealth; }}
    
    private const int StartHealth = 10;
    private const int StartOxygen = 10;
    private const int StartSpeed = 2;
    
    private const int OxygenUsage = 1;
    private const int OxygenWarningLevel = 10;
    private const int SecondOxygenWarningLevel = 2;
    private const int OxygenRefillSpeed = 10;
    private const float delay = 0.1f;
    WaitForSeconds coroutineDelay = new WaitForSeconds(delay);
    public bool IsDead { get; private set; }

    public Action<float, int> OxygenUpdated;
    public Action<float, int> HealthUpdated;

    public static PlayerStats Instance;

    public Action PlayerDied;

    public void SetToDead()
    {
        IsDead = true;
    }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;


        //equiped.EquipmentChanged += UpdateEquipment;

        maxOxygen = StartOxygen;
        maxHealth= StartHealth;
        maxSpeed= StartSpeed;

        oxygen = maxOxygen;
        health = maxHealth;
        speed = maxSpeed;


        UpdateEquipment();

    }

    private void UpdateEquipment()
    {
        Debug.Log(" -> Updating Stats from Equipment Benefits");

        //Debug.Log("PlayerStats Updated");

        // Change max values
       // maxHealth = StartHealth + equiped.Health;
        //maxOxygen = StartOxygen + equiped.Oxygen;
        //maxSpeed  = StartSpeed  + equiped.Speed;

        // Limit oxygen if removing tank
        oxygen = Math.Min(oxygen, maxOxygen);
        health = Math.Min(health, maxHealth);

        // Update visuals
        //infoHeader.SetInfo(maxHealth,maxOxygen,maxSpeed);

        // Update bars
        //OxygenUpdated.Invoke(oxygen, maxOxygen);
        //HealthUpdated.Invoke(health, maxHealth);
    }

    public void TakeDamage(int amt)
    {
        health -= amt;
        HealthUpdated.Invoke(health,maxHealth);
        if (health <= 0)
        {
            health = 0;
            IsDead = true;
            PlayerDied?.Invoke();
            uiController.ShowDeathScreen();
        }
    }
    public void ResetPlayer(bool keepstats = false) 
    {
        if (IsDead)
        {
            noOxygenSurvival = NoOxygenSurvivalMax;
            oxygen = maxOxygen;
            health = StartHealth;
        }
        IsDead = false;
        SoundMaster.Instance.PlayMusic(MusicName.IndoorMusic);
        OxygenUpdated.Invoke(oxygen, maxOxygen);
        HealthUpdated.Invoke(health, maxHealth);

    }

    public void LoadFromFile()
    {
        PlayerGameData data = SavingUtility.playerGameData;
        if (data == null) return;

        //Loading all data from file
        rb.position = SavingUtility.V3AsVector3(data.PlayerPosition);
        rb.rotation = Quaternion.LookRotation(SavingUtility.V3AsVector3(data.PlayerRotation),Vector3.up);

        health = SavingUtility.playerGameData.PlayerHealth;
        oxygen = SavingUtility.playerGameData.PlayerOxygen;

        OxygenUpdated.Invoke(oxygen,maxOxygen);
        Debug.Log("  * Updating Player *");
        Debug.Log("    Player loaded: position: "+rb.position+ " oxygen: "+oxygen+" health: "+health);
        HealthUpdated.Invoke(health,maxHealth);
    }

    public void DefineGameDataForSave()
    {
        // Player position and looking direction (Tilt is disregarder, looking direction is good enough)
        SavingUtility.playerGameData.PlayerPosition = SavingUtility.Vector3AsV3(rb.transform.position);
        SavingUtility.playerGameData.PlayerRotation = SavingUtility.Vector3AsV3(rb.transform.forward);

        // Inventory

        // Health, Oxygen
        SavingUtility.playerGameData.PlayerHealth = health;
        SavingUtility.playerGameData.PlayerOxygen = oxygen;

    }
}
