using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Wolfheat.StartMenu
{
    public enum SoundName {MenuStep, MenuError, MenuClick, MenuOver, DropItem, Shoot, HUDPositive, HUDError,
        BulletImpact,
        Drowning,
        Crafting,
        CraftComplete,
        HitMetal,
        Drill,
        PickUp,
        LowOxygen,
        EnemyShoot,
        Pinch
    }
    public enum MusicName {MenuMusic, OutDoorMusic, IndoorMusic, DeadMusic}

    [Serializable]
    public class Music : BaseSound
    {
        public MusicName name;
        public void SetSound(AudioSource source)
        {
            audioSource = source;
        }
    }

    [Serializable]
    public class Sound: BaseSound
    { 
        public SoundName name;
        public void SetSound(AudioSource source)
        {
            audioSource = source;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
        }
    }
    
    [Serializable]
    public class BaseSound
        {
        public AudioClip clip;
        [Range(0f,1f)]
        public float volume;
        [Range(0.8f, 1.2f)]
        public float pitch=1f;
        public bool loop=false;
        [HideInInspector] public AudioSource audioSource;

    }

    public class SoundMaster : MonoBehaviour
    {
        public static SoundMaster Instance { get; private set; }
        public const float MuteBoundary = 0.015f;
        public AudioMixer mixer;
        public AudioMixerGroup masterGroup;  
        public AudioMixerGroup musicMixerGroup;  
        public AudioMixerGroup SFXMixerGroup;  
        [SerializeField] private Sound[] sounds;
        [SerializeField] private Music[] musics;

        [SerializeField]private AudioClip[] swosh;
        [SerializeField]private AudioClip[] getHit;
        [SerializeField]private AudioClip[] footstep;

        private Dictionary<SoundName,Sound> soundsDictionary = new();
        private Dictionary<MusicName,Music> musicDictionary = new();
        AudioSource musicSource;
        MusicName activeMusic;
        AudioSource stepSource;

        SoundSettings soundSettings = new SoundSettings();

        public Action GlobalMuteChanged;

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            SavingUtility.LoadingComplete += LoadingComplete;
        }

        private void LoadingComplete()
        {
            Debug.Log("Loading of settings complete update");

            soundSettings = SavingUtility.gameSettingsData.soundSettings;
        }

        private void Awake()
        {
            Debug.Log("SoundMaster Start");        
            // Define all sounds
            foreach (var sound in sounds)
            {
                sound.SetSound(gameObject.AddComponent<AudioSource>());
                sound.audioSource.outputAudioMixerGroup = SFXMixerGroup;
                soundsDictionary.Add(sound.name, sound);
            }

            //Steps
            stepSource = gameObject.AddComponent<AudioSource>();
            stepSource.volume = 0.25f;
            stepSource.outputAudioMixerGroup = SFXMixerGroup;

            // And Music
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicMixerGroup;

            foreach (var music in musics)
            {
                // All music use same source (since only one will be playing at a time)
                music.SetSound(musicSource); 
                musicDictionary.Add(music.name, music);
            }

            // Play theme sound
            activeMusic = MusicName.MenuMusic;
            //PlayMusic(MusicName.MenuMusic);
        
        }

        public void PlayMusic(MusicName name)
        {
            activeMusic = name; // Leave this here so the correct music that should be played is still updated if music is reenabled

            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseMusic || !SavingUtility.HasLoaded) return;
            
            if (musicDictionary.ContainsKey(name))
            {
                if (musicDictionary[name].audioSource.isPlaying && !musicDictionary[name].loop)
                    return;
                musicSource.clip = musicDictionary[name].clip;
                musicSource.volume = musicDictionary[name].volume;
                musicSource.pitch = musicDictionary[name].pitch;
                musicSource.loop = musicDictionary[name].loop;
                musicSource.Play();
            }
            else
                Debug.LogWarning("No clip named "+name+" in dictionary.");

        }
        public void PlaySound(SoundName name, bool allowInterupt= true)
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            //Debug.Log("Play Sound: "+name);
            if (soundsDictionary.ContainsKey(name))
            {
                if (!allowInterupt && soundsDictionary[name].audioSource.isPlaying && !soundsDictionary[name].loop)
                {
                    //Debug.Log("Sound is playing and should not loop: "+name+" at:" + Time.realtimeSinceStartup);
                    return;
                }
                //Debug.Log("Start Sound: "+name);
                //Debug.Log("soundsDictionary[name].audioSource.clip: " + soundsDictionary[name].audioSource.clip   );
                soundsDictionary[name].audioSource.Play();
            }
            else 
                Debug.LogWarning("No clip named "+name+" in dictionary.");

        }

        public void FadeMusic(float time = 1f)
        {
            StartCoroutine(MusicFade(time));
        }
        public IEnumerator MusicFade(float time)
        {
            float changePerSecond = musicSource.volume / time;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= changePerSecond * Time.deltaTime;
                yield return null;
            }
            musicSource.Stop();
        }
        public void UpdateVolume()
        {
            Debug.Log("SOUNDMASTER - Updating SoundMaster's Volumes, This uses Sound Settings values");
            // Convert to dB
            mixer.SetFloat("Volume", Mathf.Log10(soundSettings.MasterVolume) * 20);
        
            //Set Music
            mixer.SetFloat("MusicVolume", Mathf.Log10(soundSettings.MusicVolume) * 20);
        
            // Set SFX
            mixer.SetFloat("SFXVolume", Mathf.Log10(soundSettings.SFXVolume) * 20);

            EnableSoundAccordingToMixersVolumes();
            
        }

        private void EnableSoundAccordingToMixersVolumes()
        {
            Debug.Log("SOUNDMASTER - Enabling Sound According to Mixer Volumes, Music volume = " + soundSettings.MusicVolume);
            //Master
            soundSettings.UseMaster = soundSettings.MasterVolume > MuteBoundary;
            soundSettings.UseMusic  = soundSettings.MusicVolume > MuteBoundary;
            soundSettings.UseSFX    = soundSettings.SFXVolume > MuteBoundary;

            Debug.Log("SOUNDMASTER - Global Sound:" + (soundSettings.GlobalMaster==true?"ON":"OFF") + "    Master: "+ soundSettings.UseMaster + " Music: " +soundSettings.UseMusic+" SFX: "+ soundSettings.UseSFX);

            if (soundSettings.GlobalMaster && soundSettings.UseMaster && soundSettings.UseMusic)
            {
                Debug.Log("SOUNDMASTER - Global and Master and Music is ON");
                if (!musicSource.isPlaying)
                {
                    Debug.Log("SOUNDMASTER - Resume Music");
                    ResumeMusic();
                }
            }
            else
            {
                Debug.Log("SOUNDMASTER - Global or Master or Music is OFF");

                if (musicSource.isPlaying)
                    musicSource.Stop();
                //Stop all SFX?
            }

        }

        public void ToggleMusic(InputAction.CallbackContext context)
        {
            ToggleMusic();
            GlobalMuteChanged?.Invoke();
        }
        public void ToggleMusic()
        {
            Debug.Log("TOGGLE MUSIC");
            soundSettings.GlobalMaster = !soundSettings.GlobalMaster;
            
            Debug.Log("Global Sound Set To:"+ (soundSettings.GlobalMaster==true?"ON":"OFF") + " Master: "+ soundSettings.UseMaster + " Music: " +soundSettings.UseMusic+" SFX: "+ soundSettings.UseSFX);
            // Update Music playing 
            if (soundSettings.GlobalMaster)
            {
                Debug.Log("GLobal master is on");
                if(soundSettings.UseMaster && soundSettings.UseMusic && !musicSource.isPlaying)
                    ResumeMusic();
            }
            else 
                if (musicSource.isPlaying)
                {
                        Debug.Log("Stopping Music from playing?");
                    musicSource.Stop();
                }
            GameSettingsData.GameSettingsUpdated?.Invoke();
        }   
        public void StopSound(SoundName name)
        {
            Debug.Log("Stop Sound: "+name);
            if (soundsDictionary.ContainsKey(name))
            {
                soundsDictionary[name].audioSource.Stop();
            }
            else
                Debug.LogWarning("No clip named " + name + " in dictionary.");
        }
        public void ResetMusic()
        {
            ResumeMusic();
            StopSound(SoundName.Drowning);
        }
        public void ResumeMusic()
        {
            Debug.Log("Resume Music");
            PlayMusic(activeMusic);
        }

        public void PlaySwoshSound()
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;
            stepSource.PlayOneShot(swosh[Random.Range(0, swosh.Length)]);
        }
        public void PlayGetHitSound()
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            // Only play foot step if last footstep is finished playing
            if (!stepSource.isPlaying)
                stepSource.PlayOneShot(getHit[Random.Range(0, getHit.Length)]);
        }
        public void PlayStepSound()
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            // Only play foot step if last footstep is finished playing
            if (!stepSource.isPlaying)
                stepSource.PlayOneShot(footstep[Random.Range(0, footstep.Length)]);
        }
        public void ReadDataFromSave()
        {
            Debug.Log("Updating Sound Volumes from saved file");
            UpdateVolume();
        }

    }
}
