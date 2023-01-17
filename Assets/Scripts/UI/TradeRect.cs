using Items;
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

        public void SetLocalRect(Proto.AppearRole role) => SureBar.text = role.name;

        public void SetRemoteRect(Proto.AppearRole role)
        {
            SureBar.text = role.name;
            for (int i = 0; i < UiRect.content.childCount; i++)
                UiRect.content.GetChild(i).GetChild(0).GetComponent<Image>().raycastTarget = false;
            GoldField.interactable = false;
            SureTog.interactable = false;
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in itemSlots)
                if (slot.Icons.childCount == 0)
                    return slot;
            return null;
        }

        public ItemSlot GetSlotByIndex(int index) => itemSlots[index];

        public void AddAllToBag()
        {
            var player = GameManager.Instance.mainPlayer.Obj;
            foreach (var slot in itemSlots)
            {
                int count = slot.Icons.childCount;
                for (int i = 0; i < count; i++)
                {
                    ItemUI itemUI = slot.Icons.GetChild(0).GetComponent<ItemUI>();
                    Proto.UpdateKnapItem proto = new()
                    {
                        Item = new()
                        {
                            Sn = itemUI.Item.Sn,
                            Id = itemUI.Item.id,
                            Index = player.AddItemToKnap(KnapType.Bag, itemUI.Item),
                            ItemType = (Proto.ItemData.Types.ItemType)itemUI.Item.itemType,
                            KnapType = Proto.ItemData.Types.KnapType.Bag
                        }
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
                    itemUI.Item.transform.SetParent(player.BagPoint, false);
                    itemUI.AddToBagUI();
                }
            }
        }

        public void RemoveAllFromBag()
        {
            foreach (var slot in itemSlots)
            {
                int count = slot.Icons.childCount;
                for (int i = 0; i < count; i++)
                {
                    ItemUI itemUI = slot.Icons.GetChild(0).GetComponent<ItemUI>();
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
                    PoolManager.Instance.Push(itemUI.Item.ObjName, itemUI.gameObject);
                    Destroy(itemUI.Item.gameObject);
                }
            }
        }
    }
}
