using UnityEngine;

public enum GameplayLogicComponentType { INTERACTION, LOCK };

[RequireComponent(typeof(InteractableGameobject))]
public abstract class GameplayLogicComponent : MonoBehaviour
{
    public abstract GameplayLogicComponentType GetGameplayLogicComponentType();
}
