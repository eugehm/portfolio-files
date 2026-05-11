using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeBranchManager : MonoBehaviour
{
    [SerializeField] List<string> prerequisites;

    void Awake()
    {
        EventBus.Subscribe<UnlockSkillEvent>(OnSkillUnlocked);
    }

    public bool MeetsPrereqs()
    {
        foreach (var prereq in prerequisites)
        {
            if (!SkillTreeManager.Instance.Unlocked(prereq))
            {
                return false;
            }
        }
        return true;
    }

    private void OnSkillUnlocked(UnlockSkillEvent unlockSkill)
    {
        if (MeetsPrereqs())
        {
            GetComponent<Image>().color = Color.white;
        }
    }
}
