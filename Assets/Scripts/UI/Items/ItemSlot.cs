using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ItemSlot : MonoBehaviour
	{
        public ItemType itemType = ItemType.None;
        public KnapType knapType = KnapType.Bag;

        public int Index { get; set; }
        public Transform Icons { get; private set; }
        public Text Count { get; private set; }

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
