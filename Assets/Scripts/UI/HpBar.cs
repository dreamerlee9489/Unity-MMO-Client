using UnityEngine;

namespace UI
{
    public class HpBar : MonoBehaviour
	{
		private RectTransform _fore;
        private float timeSpan = 6f;

        private void Awake()
        {
            _fore = transform.GetChild(0).GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if(timeSpan < 6)
                timeSpan += Time.deltaTime;
            else
            {
                timeSpan = 6;
                gameObject.SetActive(false);
            }
        }

        public void UpdateHp(int curHp, int maxHp)
        {
            if(curHp <= maxHp)
            {
                timeSpan = 0;
                gameObject.SetActive(true);
                _fore.transform.localScale = new Vector3(curHp * 1.0f / maxHp, 1, 1);
            }
        }
    }
}
