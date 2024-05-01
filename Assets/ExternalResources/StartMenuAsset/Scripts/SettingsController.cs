using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Wolfheat.StartMenu
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] Slider master;
        [SerializeField] Slider music;
        [SerializeField] Slider sfx;
        [SerializeField] Slider mouse;
        [SerializeField] GameObject muted;
        [SerializeField] TextMeshProUGUI mousePercent;
        [SerializeField] TextMeshProUGUI masterPercent;
        [SerializeField] TextMeshProUGUI musicPercent;
        [SerializeField] TextMeshProUGUI sfxPercent;
        private bool listenForSliderValues = false;

        private SoundSettings soundSettings = new SoundSettings();
        private PlayerInputSettings inputSettings = new PlayerInputSettings();
        private void OnEnable()
        {
            listenForSliderValues = false;
            Debug.Log("Settings Controller enabled, read data from file");
            //Read data from file
            soundSettings = SavingUtility.gameSettingsData.soundSettings;
            inputSettings = SavingUtility.gameSettingsData.playerInputSettings;
            if (soundSettings != null)
            {
                UpdateUISettingsPage();
            }

            UpdateSoundPercent();
            UpdateMouseSensitivityPercent();
            StartCoroutine(EnableSliderListeners());
            SoundMaster.Instance.GlobalMuteChanged += MuteChanged;
        }


        private void OnDisable()
        {
            SoundMaster.Instance.GlobalMuteChanged -= MuteChanged;
        }
        private void MuteChanged()
        {
            Debug.Log("Recieved change in mute from soundmaster");
            muted.SetActive(!SavingUtility.gameSettingsData.soundSettings.GlobalMaster);
        }

        private void UpdateUISettingsPage()
        {
            Debug.Log("SETTING MOUSE SENS TO "+ inputSettings.MouseSensitivity);
            master.value = soundSettings.MasterVolume;
            music.value = soundSettings.MusicVolume;
            sfx.value = soundSettings.SFXVolume;
            mouse.value = inputSettings.MouseSensitivity;
            muted.SetActive(!soundSettings.GlobalMaster);
        }

        private IEnumerator EnableSliderListeners()
        {
            yield return null;
            yield return null;
            listenForSliderValues = true;
        }

        public void UpdateMouseSensitivityPercent()
        {
            Debug.Log("SETTINGSCONTROLLER - Update Mouse sensitivity: " + mouse.value);

            // Update percent
            mousePercent.text = (mouse.value * 250).ToString("F0");
        }
        
        public void UpdateSoundPercent()
        {
            Debug.Log("SETTINGSCONTROLLER - Update Sound percent, Music Slider value: " + music.value);

            // Update percent
            masterPercent.text = master.value <= SoundMaster.MuteBoundary ? "MUTED" : (master.value * 100).ToString("F0");
            musicPercent.text = music.value <= SoundMaster.MuteBoundary ? "MUTED":(music.value*100).ToString("F0");
            sfxPercent.text = sfx.value <= SoundMaster.MuteBoundary ? "MUTED" : (sfx.value*100).ToString("F0");
        }

        public void UpdateInputSetting()
        {
            if (!listenForSliderValues)
            {
                Debug.Log("Slider value changed but disregarded "+mouse.value);
                return;
            }
            Debug.Log("SETTINGSCONTROLLER - Slider value changed for Mouse Input");

            SavingUtility.gameSettingsData.playerInputSettings.MouseSensitivity = mouse.value;
            UpdateMouseSensitivityPercent();
        }
        public void UpdateSound()
        {
            if (!listenForSliderValues)
            {
                Debug.Log("Slider value changed but disregarded");
                return;
            }
        
            Debug.Log("SETTINGSCONTROLLER - Slider value changed");
         

            Debug.Log("SAVINGUTILITY - update sound setting values");
            SavingUtility.gameSettingsData.SetSoundSettings(master.value, music.value, sfx.value);

            SoundMaster.Instance.UpdateVolume();
            UpdateSoundPercent();
        } 

        public void SFXSliderChange()
        {
            SoundMaster.Instance.PlaySound(SoundName.MenuClick);
        }

        public void UnMute()
        {
            Debug.Log("Request Unmute");
            soundSettings.GlobalMaster = true;
            UpdateSound();
        }
    }
}
