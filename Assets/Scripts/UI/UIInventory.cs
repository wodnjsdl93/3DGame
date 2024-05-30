using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;

    public GameObject inventoryWidow;
    public Transform slotpanl;
    public Transform dropPosition;

    [Header("Select Item")]
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedStatName;
    public TextMeshProUGUI selectedStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unequipeButton;
    public GameObject dropButton;

    private PlayerController controller;
    private PlayerCondition condition;

    ItemData selectedItem;
    int selectedItemIndex = 0;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        inventoryWidow.SetActive(false);
        slots = new ItemSlot[slotpanl.childCount];

        for(int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotpanl.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
        }

        ClearSelctedItemWindow();
    }


    void Update()
    {
        
    }

    void ClearSelctedItemWindow()
    {
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unequipeButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen())
        {
            inventoryWidow.SetActive(false);
        }
        else
        {
            inventoryWidow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWidow.activeInHierarchy;
    }

    void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;

        // 아이템이 중복 가능한지 canStack
        if(data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if(slot != null)
            {
                slot.quantity++;
                UpdateUI();
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }
        // 비어있는 슬롯 가져온다.
        ItemSlot emptySlot = GetEmptySlot();
        // 있다면
        if(emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }

        // 없다면
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    void UpdateUI()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == data && slots[i].quantity < data.MaxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot GetEmptySlot()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    void ThrowItem(ItemData data)
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    public void SelectItem(int index)
    {
        if(slots[index].item == null) return;
        {
            selectedItem = slots[index].item;
            selectedItemIndex = index;

            selectedItemName.text = selectedItem.displayName;
            selectedItemDescription.text = selectedItem.description;

            selectedStatName.text = string.Empty;
            selectedStatValue.text = string.Empty;

            for(int i = 0; i < selectedItem.consumbales.Length; i++)
            {
                selectedStatName.text += selectedItem.consumbales[i].type.ToString() + "\n";
                selectedStatValue.text += selectedItem.consumbales[i].value.ToString() + "\n";
            }

            useButton.SetActive(selectedItem.type == ItemType.Consumable);
            equipButton.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped);
            unequipeButton.SetActive(selectedItem.type == ItemType.Equipable && slots[index].equipped);
            dropButton.SetActive(true);
        }        
    }

    public void OnUseButton()
    {
        if(selectedItem.type == ItemType.Consumable)
        {
            for(int i = 0; i < selectedItem.consumbales.Length; i++)
            {
                switch(selectedItem.consumbales[i].type)
                {
                    case ConsumableType.health:
                        condition.Heal(selectedItem.consumbales[i].value);
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumbales[i].value);
                        break;
                }
            }
            RemoveSelectedItem();
        }
    }

    public void OnDropButton()
    {
        ThrowItem(selectedItem);
        RemoveSelectedItem();
    }

    void RemoveSelectedItem()
    {
        slots[selectedItemIndex].quantity--;

        if(slots[selectedItemIndex].quantity <= 0)
        {
            selectedItem = null;
            slots[selectedItemIndex].item = null;
            selectedItemIndex = -1;
            ClearSelctedItemWindow();
        }

        UpdateUI();
    }
}
