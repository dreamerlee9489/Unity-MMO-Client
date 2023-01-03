using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EntityNameBar : MonoBehaviour
    {
        public Text Name { get; set; }

        private void Awake()
        {
            Name = transform.GetChild(0).GetComponent<Text>();
        }

        private void Start()
        {
            if (transform.parent != null)
            {
                if (transform.parent.gameObject.name == "MainPlayer")
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
    }
}
