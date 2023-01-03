using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ItemSlot : MonoBehaviour
	{
        public int index;
        public Transform icons;
        public Text count;

        private void Awake()
        {
            icons = transform.Find("Icons");
            count = transform.Find("Count").GetComponent<Text>();
        }
    }
}
