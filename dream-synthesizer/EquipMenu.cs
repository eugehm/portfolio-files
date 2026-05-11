using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipMenu : MonoBehaviour
{
    public static bool EquipIsOpen = false;
    public GameObject equipMenuUI;
    InventoryManager inventory;
    public Transform content;
    ItemSlot[] slots;

    void Start()
    {
        inventory = InventoryManager.Instance;
        inventory.onEquipChangedCallback += UpdateUI;
        slots = content.GetComponentsInChildren<ItemSlot>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (EquipIsOpen)
            {

                equipMenuUI.SetActive(false);
                EquipIsOpen = false;
            }
            else
            {
                UpdateUI();
                equipMenuUI.SetActive(true);
                EquipIsOpen = true;
            }
        }
    }

    public void UpdateAllClicked()
    {
        foreach (var slot in slots)
        {
            slot.UpdateClicked();
        }
    }

    public void UpdateUI()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.EquipmentItems.Count)
            {
                slots[i].AddItem(inventory.EquipmentItems[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}
