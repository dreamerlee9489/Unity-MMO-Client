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
        public Dictionary<string, int> UiDict { get; set; } = new();

        protected override void Awake()
        {
            base.Awake();
            _goldTxt = transform.Find("GoldBar").GetChild(0).GetComponent<Text>();
            Content = transform.Find("ScrollRect").GetComponent<ScrollRect>().content;
            ItemSlot itemSlot;
            for (int i = 0; i < 30; i++)
            {
                itemSlot = Content.GetChild(i).GetComponent<ItemSlot>();
                itemSlot.Index = i;
                ItemSlots.Add(itemSlot);
            }
            Debug.Log(ItemSlots.Count);
            Close();
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in ItemSlots)
                if (slot.Icons.childCount == 0)
                    return slot;
            return null;
        }

        public void UpdateGold(int currGold)
        {
            _goldTxt.text = currGold.ToString();
        }
    }
}
