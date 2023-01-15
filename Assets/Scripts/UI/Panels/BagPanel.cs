using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BagPanel : BasePanel
	{
        private Text _goldTxt;
        
        public ItemSlot[] itemSlots = new ItemSlot[30];

        public RectTransform Content { get; set; }
        public Dictionary<int, int> UiIndexDict { get; set; } = new();

        protected override void Awake()
        {
            base.Awake();
            _goldTxt = transform.Find("GoldBar").GetChild(0).GetComponent<Text>();
            Content = transform.Find("ScrollRect").GetComponent<ScrollRect>().content;
            for (int i = 0; i < 30; i++)
            {
                itemSlots[i] = Content.GetChild(i).GetComponent<ItemSlot>();
                itemSlots[i].Index = i;
            }
            Close();
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in itemSlots)
                if (slot.Icons.childCount == 0)
                    return slot;
            return null;
        }

        public ItemSlot GetSlotByIndex(int index) => itemSlots[index];

        public void UpdateGold(int currGold)
        {
            _goldTxt.text = currGold.ToString();
        } 
    }
}
