using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer musicSource, sfxSource;
    public Slider musicSlider, sfxSlider, speedSlider, scaleSlider;
    public TMPro.TMP_Dropdown qualityDropdown, displayDropdown, textDropdown;
    public TMP_Text musicPercent, sfxPercent, speedPercent, scalePercent;
    public static UnityEvent ChangedSpeed = new UnityEvent(), ChangedZoom = new UnityEvent(), ChangedDialogue = new UnityEvent();

    private void OnEnable()
    {
        Settings.UpdateUI.AddListener(UpdateOptions);
    }

    void Start()
    {
        if (Settings.Instance != null)
        {
            musicPercent.text = Mathf.RoundToInt((Settings.Instance.musicVol + 80f) * 1.25f).ToString();
            sfxPercent.text = Mathf.RoundToInt((Settings.Instance.sfxVol + 80f) * 1.25f).ToString();
            speedPercent.text = Mathf.RoundToInt((Settings.Instance.moveSpeed / 20f) * 100).ToString();
            scalePercent.text = Mathf.RoundToInt((Settings.Instance.scaleSpeed / 0.02f) * 100).ToString();
        }
        else
        {
            musicPercent.text = "100";
            sfxPercent.text = "100";
            speedPercent.text = "50";
            scalePercent.text = "50";
        }
    }

    private void UpdateOptions()
    {
        if (Settings.Instance != null)
        {
            musicSlider.value = Settings.Instance.musicVol;
            sfxSlider.value = Settings.Instance.sfxVol;
            speedSlider.value = Settings.Instance.moveSpeed;
            scaleSlider.value = Settings.Instance.scaleSpeed;
            qualityDropdown.value = Settings.Instance.qualityIndex;
            displayDropdown.value = Settings.Instance.displayIndex;
            textDropdown.value = Settings.Instance.textIndex;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.musicVol = volume;
        }
        musicSource.SetFloat("musicVolume", volume);
        musicPercent.text = Mathf.RoundToInt((volume + 80f) * 1.25f).ToString();
    }

    public void SetSFXVolume(float volume)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.sfxVol = volume;
        }
        sfxSource.SetFloat("sfxVolume", volume);
        sfxPercent.text = Mathf.RoundToInt((volume + 80f) * 1.25f).ToString();
    }

    public void SetSpeed(float speed)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.moveSpeed = speed;
        }
        ChangedSpeed.Invoke();
        speedPercent.text = Mathf.RoundToInt((speed / 20f) * 100).ToString();
    }

    public void SetScale(float scale)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.scaleSpeed = scale;
        }
        ChangedZoom.Invoke();
        scalePercent.text = Mathf.RoundToInt((scale / 0.02f) * 100).ToString();
    }

    public void SetQuality(int index)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.qualityIndex = index;
        }
        QualitySettings.SetQualityLevel(index);
    }

    public void SetDisplay(int index)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.displayIndex = index;
        }

        if (index == 0)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.fullScreen = true;
        }
    }

    public void SetText(int index)
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.textIndex = index;
        }
        ChangedDialogue.Invoke();
    }
}
