using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Lock : GameplayLogicComponent
{
    public override GameplayLogicComponentType GetGameplayLogicComponentType()
    {
        return GameplayLogicComponentType.LOCK;
    }

    public abstract IEnumerator TryLock();
    public abstract bool CheckUnlocked();
}
