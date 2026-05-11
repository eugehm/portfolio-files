using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnPointsChange { }

public class SkillPointSystem : MonoBehaviour
{
    public static SkillPointSystem Instance;
    private int skillPoints = 0;

    private void Awake()
    {
        Instance = this;
        skillPoints = GameManager.Instance.gameParams.initialSkillPoints;
    }

    private void Start()
    {
        EventBus.Publish<OnPointsChange>(new OnPointsChange());
    }

    public void AddPoints(int amount)
    {
        skillPoints += amount;
        EventBus.Publish<OnPointsChange>(new OnPointsChange());
    }

    public int GetPoints()
    {
        return skillPoints;
    }
}
