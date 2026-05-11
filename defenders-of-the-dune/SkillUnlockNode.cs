using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillUnlockState
{
    Locked,
    Available,
    Purchasable,
    Purchased
};

public class SkillUnlockNode : MonoBehaviour
{
    [SerializeField] string id;
    [SerializeField] int cost;
    [SerializeField] GameObject border;
    [SerializeField] GameObject sprite;
    [SerializeField] List<string> prerequisites;
    public SkillUnlockState unlockState;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        EventBus.Subscribe<UnlockSkillEvent>(OnSkillUnlocked);
        EventBus.Subscribe<OnBuy>(Purchase);
        EventBus.Subscribe<OnPointsChange>(OnSkillPurchasable);
        if (unlockState == SkillUnlockState.Locked)
        {
            button.interactable = false;
        }
        button.onClick.AddListener(() => WhiteHighlight());
        button.onClick.AddListener(() => UpdateSkillInfo());
    }

    public void Purchase(OnBuy buy)
    {
        if (id != buy.id) return;

        if (unlockState == SkillUnlockState.Purchasable && SkillTreeManager.Instance.HasEnough(cost))
        {
            SkillTreeManager.Instance.Spend(cost);
            SkillTreeManager.Instance.UnlockSkill(id);
            RefreshStatus();
        }
        else if (SkillTreeManager.Instance.skillsUnlockStatus.ContainsKey(id))
        {
            Debug.Log($"Cannot afford skill ID {id} yet.");
        }
        else
        {
            Debug.Log($"Cannot afford equipment ID {id} yet.");
        }
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

    private void OnSkillUnlocked(UnlockSkillEvent _)
    {
        RefreshStatus();
    }

    private void OnSkillPurchasable(OnPointsChange _)
    {
        RefreshStatus();
    }

    public void WhiteHighlight()
    {
        foreach (SkillUnlockNode button in SkillTreeMenu.Instance.GetSkillButtons())
        {
            if (button.unlockState != SkillUnlockState.Purchasable)
            {
                button.border.GetComponent<Image>().enabled = false;
            }
            else
            {
                button.border.GetComponent<Image>().enabled = true;
                button.border.GetComponent<Image>().color = Color.green;
            }
        }

        border.GetComponent<Image>().enabled = true;
        border.GetComponent<Image>().color = Color.white;
    }

    public void RefreshStatus()
    {
        if (SkillTreeManager.Instance.Unlocked(id))
        {
            unlockState = SkillUnlockState.Purchased;
            button.interactable = true;
            sprite.GetComponent<Image>().color = Color.white;
            border.GetComponent<Image>().enabled = false;
            border.GetComponent<Image>().color = Color.white;
        }
        else if (MeetsPrereqs() && SkillTreeManager.Instance.HasEnough(cost))
        {
            unlockState = SkillUnlockState.Purchasable;
            button.interactable = true;
            border.GetComponent<Image>().enabled = true;
            border.GetComponent<Image>().color = Color.green;
        }
        else if (MeetsPrereqs())
        {
            unlockState = SkillUnlockState.Available;
            button.interactable = true;
            border.GetComponent<Image>().enabled = false;
            border.GetComponent<Image>().color = Color.white;
        }
        else
        {
            unlockState = SkillUnlockState.Locked;
            button.interactable = false;
        }
    }

    private void UpdateSkillInfo()
    {
        SkillTreeMenu.Instance.UpdateNameAndCost(id, cost);
    }
}
