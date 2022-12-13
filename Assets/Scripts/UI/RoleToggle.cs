using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoleToggle : MonoBehaviour
    {
        public Text Name { get; set; }
        public Text Level { get; set; }
        public ulong Id { get; set; }

        private void Awake()
        {
            Name = transform.Find("Name").GetComponent<Text>();
            Level = transform.Find("Level").GetComponent<Text>();
        }

        private void Start()
        {
            GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
        }
    }
}
