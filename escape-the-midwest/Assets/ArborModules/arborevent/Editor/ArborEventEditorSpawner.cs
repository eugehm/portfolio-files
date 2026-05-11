using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
class ArborEventEditorSpawner
{
    static ArborEventEditorSpawner()
    {
        RegisterModuleDefine("ARBOREVENT");
        EditorApplication.update += EditorUpdate;
    }

    static void RegisterModuleDefine(string symbol)
    {
        symbol = symbol.ToUpperInvariant().Trim();
        string current_defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup
                         );
        HashSet<string> current_defines_set = new HashSet<string>(current_defines.Split(';'));
        current_defines_set.Add(symbol);
        current_defines = "";
        foreach (string define in current_defines_set)
            current_defines += define + ";";

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            current_defines
        );
    }

    static void EditorUpdate()
    {
        if (Application.isPlaying)
            return;

        EnforcePrefabExistsInScene("_ArborEventManager");
    }

    static bool ObjectInSceneWithName(string name)
    {
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                continue;

            if (go.name.Equals(name))
                return true;
        }

        return false;
    }

    static void EnforcePrefabExistsInScene(string name)
    {
        if (GameObject.Find(name) == null)
        {
            GameObject application_prefab = Resources.Load<GameObject>(name);
            if (application_prefab == null)
            {
                Debug.LogError("Scene-Critical prefab could not be loaded: [" + name + "]");
                return;
            }

            GameObject new_required_object = PrefabUtility.InstantiatePrefab(application_prefab) as GameObject;
            new_required_object.name = name;
        }
    }
}