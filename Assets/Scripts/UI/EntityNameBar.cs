using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EntityNameBar : MonoBehaviour
    {
        public Text Name { get; set; }
        public HpBar HpBar { get; set; }

        private void Awake()
        {
            Name = transform.Find("Name").GetComponent<Text>();
            HpBar = transform.Find("HpBar").GetComponent<HpBar>();
        }

        private void Start()
        {
            if (transform.parent != null)
            {
                if (transform.parent.gameObject.name == "mainPlayer")
                {
                    Name.color = Color.green;
                    return;
                }
                switch (transform.parent.tag)
                {
                    case "Player":
                    case "NPC":
                        Name.color = Color.yellow;
                        break;
                    case "Enemy":
                        Name.color = Color.red;
                        break;
                    default:
                        break;
                }
            }
        }

        private void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }

        public void ChangeCamp()
        {
            switch (transform.parent.tag)
            {
                case "Player":
                case "NPC":
                    Name.color = Color.yellow;
                    break;
                case "Enemy":
                    Name.color = Color.red;
                    break;
                default:
                    break;
            }
        }
    }
}
