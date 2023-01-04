using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ItemSlot : MonoBehaviour
	{
        public int index = 0;
        public ItemType slotType = ItemType.None;
        
        public Transform Icons { get; set; }
        public Text Count { get; set; }

        private void Awake()
        {
            Icons = transform.Find("Icons");
            Count = transform.Find("Count").GetComponent<Text>();
        }

        public ItemType GetIconType()
        {
            if(Icons.childCount > 0)
                return Icons.GetChild(0).GetComponent<ItemUI>().Item.itemType;
            return ItemType.None;
        }
    }
}
