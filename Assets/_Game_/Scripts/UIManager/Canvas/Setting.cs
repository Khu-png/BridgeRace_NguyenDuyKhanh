using UnityEngine;
using UnityEngine.UI;

public class Setting : UICanvas
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    protected override void Awake()
    {
        base.Awake();
        CacheSliders();
        SetupSlider(musicSlider, MusicVolumeKey, OnMusicVolumeChanged);
        SetupSlider(sfxSlider, SfxVolumeKey, OnSfxVolumeChanged);
    }

    private void OnEnable()
    {
        ApplySavedVolumes();
    }

    public void OnClickClose()
    {
        CloseDirectly();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        AudioManager.Instance?.SetMusicVolume(volume);
    }

    public void OnSfxVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        AudioManager.Instance?.SetSFXVolume(volume);
    }

    private void CacheSliders()
    {
        if (musicSlider == null)
        {
            musicSlider = FindChildSlider("Music Slider");
        }

        if (sfxSlider == null)
        {
            sfxSlider = FindChildSlider("SFX Slider");
        }
    }

    private Slider FindChildSlider(string sliderName)
    {
        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i].name == sliderName)
            {
                return sliders[i];
            }
        }

        return null;
    }

    private void SetupSlider(Slider slider, string key, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, 1f));
        slider.onValueChanged.RemoveListener(callback);
        slider.onValueChanged.AddListener(callback);
    }

    private void ApplySavedVolumes()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
        }

        AudioManager.Instance?.SetMusicVolume(musicVolume);
        AudioManager.Instance?.SetSFXVolume(sfxVolume);
    }
}
