using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    public Sound[] musicSound, sfxSound;
    public AudioSource musicSource, sfxSource;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        ApplySavedVolumes();
    }

    public void Start()
    {
        PlayMusic("Theme");
    }    

    public void PlayMusic(string name)
    {
        Sound s = System.Array.Find(musicSound, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = System.Array.Find(sfxSound, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        sfxSource.PlayOneShot(s.clip);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    private void ApplySavedVolumes()
    {
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
    }
}
