using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TradeRect : MonoBehaviour
	{
        private ScrollRect _tradeRect;
		private InputField _glodField;
        
        public ItemSlot[] itemSlots = new ItemSlot[12];
        
        public Text SureBar { get; private set; }
        public Toggle SureTog { get; private set; }
        public Dictionary<int, int> UiIndexDict { get; set; } = new();

        private void Awake()
        {
            _tradeRect = GetComponent<ScrollRect>();
            _glodField = transform.GetComponentInChildren<InputField>();
            SureBar = transform.Find("SureBar").GetComponent<Text>();
            SureTog = transform.GetComponentInChildren<Toggle>();
            for (int i = 0; i < 12; i++)
            {
                itemSlots[i] = _tradeRect.content.GetChild(i).GetComponent<ItemSlot>();
                itemSlots[i].Index = i;
            }
        }

        public void SetLocalRect(string name)
        {
            SureBar.text = name;
        }

        public void SetRemoteRect(string name)
        {
            SureBar.text = name;
            for (int i = 0; i < _tradeRect.content.childCount; i++)
                _tradeRect.content.GetChild(i).GetChild(0).GetComponent<Image>().raycastTarget = false;
            _glodField.interactable = false;
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
    }
}
