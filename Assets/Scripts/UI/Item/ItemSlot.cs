using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ItemSlot : MonoBehaviour
	{
        public int Index { get; set; }
        public Transform Icons { get; set; }
        public Text Count { get; set; }

        private void Awake()
        {
            Icons = transform.Find("Icons");
            Count = transform.Find("Count").GetComponent<Text>();
        }
    }
}
