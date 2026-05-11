using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using IngameDebugConsole;
using UnityEngine.UI;

public class SkillTreeMenu : Popup
{
    public static SkillTreeMenu Instance;
    [SerializeField] GameObject skillTreeMenu;
    [SerializeField] GameObject buyButton;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TextMeshProUGUI SPText;
    private string buyingID = "";
    private int currCost = 0;

    void Awake()
    {
        Instance = this;
        InputAction showTree = GameManager.Instance.gameInput.Game.SkillTreeMenu;
        showTree.performed += (_) => TogglePopup();
        EventBus.Subscribe<OnPointsChange>(OnUpdateSPUI);
    }

    void Start()
    {
        UpdateSPUI();

        // block skill tree menu when the console is visible
        DebugLogManager.Instance.OnLogWindowShown += () => GameManager.Instance.gameInput.Game.SkillTreeMenu.Disable();
        DebugLogManager.Instance.OnLogWindowHidden += () => GameManager.Instance.gameInput.Game.SkillTreeMenu.Enable();
    }

    public override void OnShowPopup()
    {
        base.OnShowPopup();
        ClearInfo();
        GetComponent<Canvas>().enabled = visible;
        Time.timeScale = 0f;
        EventBus.Publish(new OnResearchTreeOpen(visible));
    }

    public override void OnHidePopup()
    {
        base.OnHidePopup();
        GetComponent<Canvas>().enabled = visible;
        Time.timeScale = 1f;
        EventBus.Publish(new OnResearchTreeOpen(visible));
    }

    public void ChangeInfo(string newInfo)
    {
        infoText.text = newInfo;
    }

    public void UpdateNameAndCost(string ID, int cost)
    {
        SkillData skillData = Globals.SKILL_DICT[ID];
        nameText.text = skillData.GetReferencedName();
        if (cost == 1) pointsText.text = "point";
        else pointsText.text = "points";
        currCost = cost;
        costText.text = cost.ToString();
        UpdateButtonState();
    }

    public void ClearInfo()
    {
        infoText.text = "";
        nameText.text = "";
        costText.text = "";
        pointsText.text = "";
        currCost = 0;
        buyButton.SetActive(false);

        foreach (SkillUnlockNode button in GetSkillButtons())
        {
            button.RefreshStatus();
        }
    }

    public void IfBuying(string id)
    {
        if (!SkillTreeManager.Instance.Unlocked(id))
        {
            buyButton.SetActive(true);
            buyingID = id;
        }
        else
        {
            buyButton.SetActive(false);
            buyingID = "";
        }
    }

    public void Buy()
    {
        SkillTreeManager.Instance.Purchase(buyingID);

        // check if skill was unlocked to hide button
        if (SkillTreeManager.Instance.Unlocked(buyingID))
        {
            buyButton.SetActive(false);
        }
    }

    void UpdateButtonState()
    {
        TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
        Button buyButtonComponent = buyButton.GetComponent<Button>();

        if (SkillTreeManager.Instance.HasEnough(currCost))
        {
            buttonText.color = buttonText.color.WithAlpha(1f);
            buyButtonComponent.interactable = true;
        }
        else
        {
            buttonText.color = buttonText.color.WithAlpha(0.6f);
            buyButtonComponent.interactable = false;
        }
    }

    private void OnUpdateSPUI(OnPointsChange pointsChange)
    {
        UpdateSPUI();
        UpdateButtonState();
    }

    private void UpdateSPUI()
    {
        SPText.text = SkillPointSystem.Instance.GetPoints().ToString();
    }

    public SkillUnlockNode[] GetSkillButtons()
    {
        return GetComponentsInChildren<SkillUnlockNode>();
    }

    public class OnResearchTreeOpen
    {
        public bool isOpen;
        public OnResearchTreeOpen(bool isOpen)
        {
            this.isOpen = isOpen;
        }
    }
}
