using Items;
using Manage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KnapPanel : BasePanel
	{
        private Text _goldTxt;

        public RectTransform Content { get; set; }
        public List<ItemSlot> ItemSlots { get; set; } = new();
        public Dictionary<int, int> UiIndexDict { get; set; } = new();

        protected override void Awake()
        {
            base.Awake();
            _goldTxt = transform.Find("GoldBar").GetChild(0).GetComponent<Text>();
            Content = transform.Find("ScrollRect").GetComponent<ScrollRect>().content;
            ItemSlot itemSlot;
            for (int i = 0; i < 30; i++)
            {
                itemSlot = Content.GetChild(i).GetComponent<ItemSlot>();
                itemSlot.index = i;
                ItemSlots.Add(itemSlot);
            }
            Close();
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in ItemSlots)
                if (slot.Icons.childCount == 0)
                    return slot;
            return null;
        }

        public ItemSlot GetSlotByIndex(int index) => ItemSlots[index];

        public void UpdateGold(int currGold)
        {
            _goldTxt.text = currGold.ToString();
        }

        public void UpdateUiIndex(GameItem item, ItemSlot newSlot)
        {
            UiIndexDict[item.GetHashCode()] = newSlot.index;
            Proto.UpdateKnapItem proto = new() { Item = new() };
            switch (item.itemType)
            {
                case ItemType.Potion:
                    proto.Item.Type = Proto.ItemData.Types.ItemType.Potion;
                    break;
                case ItemType.Weapon:
                    proto.Item.Type = Proto.ItemData.Types.ItemType.Weapon;                    
                    break;
                default:
                    break;
            }
            proto.Item.Sn = item.Sn;
            proto.Item.Id = item.id;
            proto.Item.Index = newSlot.index;
            NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
        }
    }
}
