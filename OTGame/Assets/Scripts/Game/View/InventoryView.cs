using NotSoSimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class ItemView
{
    public JSONNode data;
    public GameObject view;
}

public class InventoryView : MonoBehaviour
{
    GameObject equipmentView;
    Transform inventory;
    GameObject itemSlotTemplate;

    Dictionary<int, ItemView> items = new Dictionary<int, ItemView>();

    void Start()
    {
        UICommon.FadeIn(gameObject);
    }

    private GameObject DisplayItem(JSONNode item)
    {
        var itemSlot = Instantiate(itemSlotTemplate, inventory, false);
        itemSlot.SetActive(true);
        itemSlot.name = "Item [" + item["name"].AsString + "]";
        
        var itemImage = itemSlot.transform.Find("ItemImage").GetComponent<RawImage>();
        itemImage.texture = Resources.Load<Texture>("Sprites/Items/" + item["name"].AsString);

        var equipButton = itemSlot.transform.Find("Equip").GetComponent<Button>();
        if (item["canBeEquipped"].AsBool.Value)
            equipButton.onClick.AddListener(() => { EquipItem(item); });
        else equipButton.gameObject.SetActive(false);

        return itemSlot;
    }

    private GameObject DisplayItemEquipped(JSONNode item)
    {
        foreach(Transform obj in equipmentView.transform)
        {
            if (obj.name.StartsWith("Sword"))
            {
                var itemImage = obj.Find("ItemImage").gameObject;
                if (!itemImage.activeSelf)
                {
                    itemImage.SetActive(true);
                    itemImage.GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/Items/" + item["name"].AsString);

                    var equipButton = obj.Find("Unequip").GetComponent<Button>();
                    equipButton.onClick.RemoveAllListeners();
                    equipButton.onClick.AddListener(() => { UnequipItem(item); });
                    equipButton.gameObject.SetActive(true);

                    return obj.gameObject;
                } 
            }
        }

        return null;
    }

    public void Initialize(JSONArray items)
    {
        equipmentView = transform.Find("background/Equipment").gameObject;
        inventory = transform.Find("background/Scroll View/Viewport/Content/ItemsList");

        // for (int i = 1; i < inventory.transform.childCount; i++)
        //     Destroy(questList.transform.GetChild(1).gameObject);

        itemSlotTemplate = inventory.Find("ItemSlotTemplate").gameObject;
        itemSlotTemplate.SetActive(false);
        foreach (var item in items)
        {
            GameObject itemView;
            if (item["isEquipped"].AsBool.Value)
                itemView = DisplayItemEquipped(item);
            else itemView = DisplayItem(item);

            this.items.Add(item["id"].AsInt.Value, new ItemView() { data = item, view = itemView });
        }
    }

    public void EquipItem(JSONNode item)
    {
        GameHub.Invoke("EquipItem", item["id"].AsInt.Value);
    }

    public void UnequipItem(JSONNode item)
    {
        GameHub.Invoke("EquipItem", item["id"].AsInt.Value);
    }

    public void OnItemEquipped(int id)
    {
        var item = items[id];

        var newView = DisplayItemEquipped(item.data);

        Destroy(item.view);
        item.view = newView;
    }

    public void OnItemUnequipped(int id)
    {
        var item = items[id];

        var newView = DisplayItem(item.data);

        if (item.view != null)
        {
            item.view.transform.Find("ItemImage").gameObject.SetActive(false);
            item.view.transform.Find("Unequip").gameObject.SetActive(false);
        }
        item.view = newView;
    }
}
