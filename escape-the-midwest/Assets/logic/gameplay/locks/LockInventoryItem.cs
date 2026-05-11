using System.Collections;
using UnityEngine;

public class LockInventoryItem : Lock
{
    [SerializeField] string required_item_name = "";
    [SerializeField] bool once_unlocked_stay_unlocked = true;
    bool locked = true;

    private void Start()
    {
        if (required_item_name == null || required_item_name.Trim().ToLowerInvariant() == "")
        {
            Utilities.LogErrorForStudents("LockInventoryItem", gameObject, "You forgot to specify a required inventory item for the LockInventoryItem.");
            return;
        }
    }

    public override bool CheckUnlocked()
    {
        return !locked;
    }

    public override IEnumerator TryLock()
    {
        if(once_unlocked_stay_unlocked)
        {
            if (locked == false)
                yield break;
        }

        bool success = InventoryItem.ConsumeItem(required_item_name);
        if (success)
        {
            locked = false;
        }
        else
        {
            locked = true;
            ToastManager.RequestToast($"Item Required : {required_item_name}", ToastType.NORMAL);
        }

        yield break;
    }
}
