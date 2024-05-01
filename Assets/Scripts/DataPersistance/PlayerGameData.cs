using System;
using System.Collections.Generic;
using System.Numerics;

public class AchievementData
{
    public bool[] Data { get; set; } = new bool[0];
}

public class MissionsSaveData
{
    public Dictionary<int,MissionSaveData> Data { get; set; } = new Dictionary<int, MissionSaveData>();
}

public class MissionSaveData
{
    public bool everCompleted = false;
    public bool active = true;
    public int amount = 0;
    public DateTime lastCompletion = DateTime.MinValue;


    public static Action MissionUpdate;

    public void SetMissionCompletionInfo()
    {
        // Set new last completiontime
        lastCompletion = DateTime.UtcNow;
        everCompleted = true;
        amount = 0;

        MissionUpdate?.Invoke();
    }

    public bool CompleteStepForMission(int completeAmount)
    {
        amount++;

        // Invoke if not completed (if completed UpdateMissionCompletion will be called which invokes the save)
        MissionUpdate?.Invoke();

        return amount >= completeAmount;
    }

}

[Serializable]
public class LightSettings
{
    public float LightIntensity { get; set; } = 1;
}

[Serializable]
public class PlayerInputSettings
{
    public float MouseSensitivity { get; set; } = 0.36f;
}

[Serializable]
public class SoundSettings
{
    public bool GlobalMaster{ get; set; } = true;
    public bool UseMaster { get; set; } = true;
    public bool UseMusic { get; set; } = true;
    public float MasterVolume { get; set; } = 0.5f;
    public float MusicVolume { get; set; } = 0.2f;
    public bool UseSFX { get; set; } = true;
    public float SFXVolume { get; set; } = 0.4f;
}


[Serializable]
public class SaveEnemy : SaveItem
{
    public int health; 
}

[Serializable]
public class InventorySave
{
    public int[] resources;
    public InventorySaveItem[] inventorySaveItems;
}

[Serializable]
public class InventorySaveItem
{
    public int mainType;
    public int subType;
    public int[] gridPosition;
}

[Serializable]
public class SaveItem
{
    public int id; 
    public float[] position; 
    public float[] rotation;
    public SaveItem() { }
}

[Serializable]
public class SaveDroppedItem
{
    public int mainType; 
    public int subType; 
    public float[] position; 
    public float[] rotation;
}

[Serializable]
public class PlayerGameData
{
    public PlayerGameData()
    {
        PlayTime = 0;
    }

    // Players Inventory

    // Position
    public float[] PlayerPosition { get; set; }
    public SaveItem[][] Destructables { get; set; }
    public SaveItem[][] Resources { get; set; }
    public SaveEnemy[][] Enemies { get; set; }
    
    // Totals
    public int PlayTime { get; set; }
    public float[] PlayerRotation { get; set; }
    public int PlayerHealth { get; set; }
    public float PlayerOxygen { get; set; }
    public InventorySave Inventory { get; set; }
    public SaveDroppedItem[] Pickables { get; set; }

    // Action Events
    public static Action InventoryUpdate;
    public static Action MinuteWatched;

    public void AddPlayTimeMinutes(int amt)
    {
        PlayTime += amt;
        MinuteWatched?.Invoke();
    }
}

[Serializable]
public class GameSettingsData
{
    // General Game Settings
    public int ActiveTouchControl { get; set; } // Having these private set wont let the load method write these values
    public int CameraPos { get; set; } // Having these private set wont let the load method write these values

    public SoundSettings soundSettings = new SoundSettings();
    public LightSettings lightSettings = new LightSettings();
    public PlayerInputSettings playerInputSettings = new PlayerInputSettings(); // Use shake etc

    // Action Events
    public static Action GameSettingsUpdated;

    // General Settings - methods
    public void SetSoundSettings(float master, float music, float SFX,bool setFromFile=false)
    {
        soundSettings.MasterVolume = master;
        soundSettings.MusicVolume = music;
        soundSettings.SFXVolume = SFX;
        if(!setFromFile)
            GameSettingsUpdated?.Invoke();
    }
}
