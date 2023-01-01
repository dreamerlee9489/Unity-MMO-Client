using Proto;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Manage
{
    public class UIManager : MonoSingleton<UIManager>
    {
        private CanvasGroup _cutImage;
        private readonly Dictionary<string, BasePanel> _panelDict = new();

        protected override void Awake()
        {
            base.Awake();
            _cutImage = transform.Find("CutImage").GetComponent<CanvasGroup>();
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.P))
            {
                PropPanel propPanel = FindPanel<PropPanel>();
                if (propPanel.gameObject.activeSelf)
                    propPanel.Close();
                else
                    propPanel.Open();
            }
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void DisconnectCallback(EAppType appType)
        {
        }

        public T FindPanel<T>() where T : BasePanel
        {
            if (_panelDict.ContainsKey(typeof(T).Name))
                return _panelDict[typeof(T).Name].GetComponent<T>();
            return null;
        }

        public void AddPanel(string type, BasePanel panel)
        {
            if (!_panelDict.ContainsKey(type))
                _panelDict.Add(type, panel);
        }

        public IEnumerator FadeAlpha()
        {
            _cutImage.alpha = 1.0f;
            yield return new WaitForSeconds(1f);
            foreach (var enemy in GameManager.Instance.ActiveWorld.Enemies)
            {
                RequestSyncEnemy proto = new()
                {
                    PlayerSn = GameManager.Instance.MainPlayer.Sn,
                    EnemyId = enemy.id
                };
                NetManager.Instance.SendPacket(MsgId.C2SRequestSyncEnemy, proto);
            }
            WaitForSeconds sleep = new(0.02f);
            while (_cutImage.alpha > 0)
            {
                _cutImage.alpha -= 0.01f;
                yield return sleep;
            }
        }
    }
}
