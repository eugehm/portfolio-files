using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class QualitySettingsManager : MonoBehaviour
{
    static QualitySettingsManager instance;

    GameObject graphy;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        UniversalRenderPipeline.asset.msaaSampleCount = 4;

        graphy = transform.Find("[Graphy]").gameObject;
        if (graphy == null)
            Debug.LogError("Failed to locate graphy in scene [" + SceneManager.GetActiveScene().name + "]");
        graphy.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
            CheckCheatCodes();

        if (WebManager.IsTabCurrentlyActive() && Application.isFocused)
            CheckForPerformanceLoss();
    }


    public enum AutomatedQualitySettingsState { SIXTY, THIRTY };
    AutomatedQualitySettingsState current_automated_quality_settings_state = AutomatedQualitySettingsState.SIXTY;

    float sixty_fps_timer = 1.0f;
    float thirty_fps_timer = 1.0f;
    
    void CheckForPerformanceLoss()
    {
        /* Automatically degrade quality settings if we detect sustained low performance */
        float current_fps = 1.0f / Time.deltaTime;

        if (current_automated_quality_settings_state == AutomatedQualitySettingsState.SIXTY)
        {
            if (current_fps <= 55.0f)
                sixty_fps_timer -= Time.deltaTime;
            else
                sixty_fps_timer = 1.0f;

            if (sixty_fps_timer <= 0.0f)
            {
                UniversalRenderPipeline.asset.msaaSampleCount = 0;
                UniversalRenderPipeline.asset.renderScale = 0.75f;

                current_automated_quality_settings_state = AutomatedQualitySettingsState.THIRTY;
                Application.targetFrameRate = 30;
            }
        } 

        else if (current_automated_quality_settings_state == AutomatedQualitySettingsState.THIRTY)
        {
            if (current_fps < 29.0f)
                thirty_fps_timer -= Time.deltaTime;
            else
                thirty_fps_timer = 1.0f;

            if (thirty_fps_timer <= 0.0f)
            {
                UniversalRenderPipeline.asset.renderScale = UniversalRenderPipeline.asset.renderScale - 0.1f;
                UniversalRenderPipeline.asset.msaaSampleCount = 0;
                thirty_fps_timer = 1.0f;
            }
        }
        
    }

    void CheckCheatCodes()
    {
        // AA
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            var currentasset = UniversalRenderPipeline.asset;
            if (currentasset.msaaSampleCount == 4)
                currentasset.msaaSampleCount = 2;
            else if (currentasset.msaaSampleCount == 2)
                currentasset.msaaSampleCount = 0;
            else if (currentasset.msaaSampleCount == 0)
                currentasset.msaaSampleCount = 4;
        }

        // Graphy
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (graphy.activeSelf)
                graphy.SetActive(false);
            else
                graphy.SetActive(true);
        }

        // Dynamic Resolution
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (UniversalRenderPipeline.asset.renderScale == 1.0f)
                UniversalRenderPipeline.asset.renderScale = 0.5f;
            else
                UniversalRenderPipeline.asset.renderScale = 1.0f;
        }
    }
}
