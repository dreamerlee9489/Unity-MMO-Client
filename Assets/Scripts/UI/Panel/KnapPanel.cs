using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KnapPanel : BasePanel
	{
        public RectTransform content;
        public ItemSlot[] itemSlots = new ItemSlot[30];
        public Dictionary<string, int> uiDict = new();

        protected override void Awake()
        {
            base.Awake();
            content = transform.GetChild(1).GetComponent<ScrollRect>().content;
            for (int i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i] = content.GetChild(i).GetComponent<ItemSlot>();
                itemSlots[i].index = i;
            }
            Close();
        }

        public ItemSlot GetFirstEmptySlot()
        {
            foreach (var slot in itemSlots)
                if (slot.icons.childCount == 0)
                    return slot;
            return null;
        }
    }
}
