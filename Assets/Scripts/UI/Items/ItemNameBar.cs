using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemNameBar : MonoBehaviour
	{
		public Text Name { get; set; }

        private void Awake()
        {
            Name = transform.GetChild(0).GetComponent<Text>();
        }

        private void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
