using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }
    public float musicVol = 0, sfxVol = 0, moveSpeed = 10f, scaleSpeed = 0.01f;
    public int qualityIndex = 3, displayIndex = 0, textIndex = 1;
    public static UnityEvent UpdateUI = new UnityEvent();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChange;
    }

    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        UpdateUI.Invoke();
    }
}
