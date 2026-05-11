using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct UnlockSkillEvent
{
    public SkillData skill;

    public UnlockSkillEvent(SkillData skill)
    {
        this.skill = skill;
    }
}

public struct OnBuy
{
    public string id;
    public OnBuy(string id_in)
    {
        id = id_in;
    }
}

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance;
    public Dictionary<string, bool> skillsUnlockStatus = new();
    [ReadOnly] public bool unlocksEnabled;

    private void Awake()
    {
        Instance = this;
        unlocksEnabled = GameManager.Instance.gameParams.enableUnlocks;
        SetDefaultUnlockStatus();
    }

    void SetDefaultUnlockStatus()
    {
        foreach (var s in Globals.SKILL_DATA)
        {
            skillsUnlockStatus[s.id] = s.defaultUnlocked;
        }
    }

    public void UnlockSkill(string id)
    {
        if (skillsUnlockStatus.ContainsKey(id) && !skillsUnlockStatus[id])
        {
            skillsUnlockStatus[id] = true;
            Debug.Log($"Unlocked skill ID {id}!");
            EventBus.Publish(new UnlockSkillEvent(Globals.SKILL_DICT[id]));
        }
    }

    public bool Unlocked(string id)
    {
        // check if everything's unlocked by default
        if (!unlocksEnabled) return true;

        // otherwise, check if unlocked
        if (skillsUnlockStatus.ContainsKey(id))
        {
            return skillsUnlockStatus[id];
        }
        return false;
    }

    public bool HasEnough(int cost)
    {
        return SkillPointSystem.Instance.GetPoints() >= cost;
    }

    public void Spend(int cost)
    {
        SkillPointSystem.Instance.AddPoints(cost * -1);
    }

    public void Purchase(string id)
    {
        EventBus.Publish<OnBuy>(new OnBuy(id));
    }

    public List<string> SaveData()
    {
        // return list of unlocked skill IDs
        return skillsUnlockStatus
            .Where(x => x.Value == true)
            .Select(x => x.Key)
            .ToList();
    }

    public void LoadData(List<string> unlockedSkills)
    {
        foreach (var id in unlockedSkills)
        {
            UnlockSkill(id);
        }
    }
}
