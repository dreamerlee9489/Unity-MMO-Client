using Manage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TradeRect : MonoBehaviour
    {
        public ItemSlot[] itemSlots = new ItemSlot[12];

        public Text SureBar { get; set; }
        public Toggle SureTog { get; set; }
        public InputField GoldField { get; set; }
        public ScrollRect UiRect { get; set; }
        public Dictionary<int, int> UiIndexDict { get; set; } = new();

        private void Awake()
        {
            UiRect = GetComponent<ScrollRect>();
            GoldField = transform.GetComponentInChildren<InputField>();
            SureBar = transform.Find("SureBar").GetComponent<Text>();
            SureTog = transform.GetComponentInChildren<Toggle>();
            for (int i = 0; i < 12; i++)
            {
                itemSlots[i] = UiRect.content.GetChild(i).GetComponent<ItemSlot>();
                itemSlots[i].Index = i;
                itemSlots[i].knapType = Items.KnapType.Trade;
            }
        }

        public void InitLocalRect(Proto.AppearRole role)
        {
            SureBar.text = role.name;
            SureTog.isOn = false;
            SureTog.interactable = true;
            GoldField.interactable = true;
            GoldField.text = "";
            for (int i = 0; i < UiRect.content.childCount; i++)
            {
                UiRect.content.GetChild(i).GetChild(0).GetComponent<Image>().raycastTarget = true;
                UiRect.content.GetChild(i).GetChild(1).GetComponent<Text>().text = "";
            }
        }

        public void InitRemoteRect(Proto.AppearRole role)
        {
            SureBar.text = role.name;
            SureTog.isOn = false;
            SureTog.interactable = false;
            GoldField.interactable = false;
            GoldField.text = "";
            for (int i = 0; i < UiRect.content.childCount; i++)
            {
                UiRect.content.GetChild(i).GetChild(0).GetComponent<Image>().raycastTarget = false;
                UiRect.content.GetChild(i).GetChild(1).GetComponent<Text>().text = "";
            }
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in itemSlots)
                if (slot.Icons.childCount == 0)
                    return slot;
            return null;
        }

        public ItemSlot GetSlotByIndex(int index) => itemSlots[index];

        public void AddRemoteItems()
        {
            var player = GameManager.Instance.mainPlayer.Obj;
            foreach (var slot in itemSlots)
            {
                int count = slot.Icons.childCount;
                for (int i = 0; i < count; i++)
                {
                    ItemUI itemUI = slot.Icons.GetChild(0).GetComponent<ItemUI>();
                    itemUI.AddToBagUI();
                    itemUI.Item.transform.SetParent(player.BagPoint);
                    Proto.UpdateKnapItem proto = new()
                    {
                        Item = new()
                        {
                            Sn = itemUI.Item.Sn,
                            Id = itemUI.Item.id,
                            ItemType = (Proto.ItemData.Types.ItemType)itemUI.Item.itemType,
                            Index = player.AddItemToKnap(Items.KnapType.Bag, itemUI.Item),
                            KnapType = Proto.ItemData.Types.KnapType.Bag
                        }
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
                }
            }
        }

        public void RemoveLocalItems()
        {
            foreach (var slot in itemSlots)
            {               
                int count = slot.Icons.childCount;
                if (count > 0)
                    UIManager.Instance.GetPanel<BagPanel>().UiIndexDict.Remove(slot.Icons.GetChild(0).GetComponent<ItemUI>().Item.GetHashCode());
                for (int i = 0; i < count; i++)
                {
                    ItemUI itemUI = slot.Icons.GetChild(0).GetComponent<ItemUI>();
                    PoolManager.Instance.Push(itemUI.Item.ObjName, itemUI.gameObject);
                    Proto.UpdateKnapItem proto = new()
                    {
                        Item = new()
                        {
                            Sn = itemUI.Item.Sn,
                            Id = itemUI.Item.id,
                            Index = -1,
                            ItemType = (Proto.ItemData.Types.ItemType)itemUI.Item.itemType,
                            KnapType = Proto.ItemData.Types.KnapType.World
                        }
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
                    Destroy(itemUI.Item.gameObject);
                }
            }
        }
    }
}
