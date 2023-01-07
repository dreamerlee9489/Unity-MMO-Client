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

        public Transform tempSlot;

        protected override void Awake()
        {
            base.Awake();
            _cutImage = transform.Find("CutImage").GetComponent<CanvasGroup>();
            tempSlot = transform.Find("TempSlot");
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void Update()
        {
            if(GameManager.Instance.CurrWorld)
            {
                if (Input.GetKeyDown(KeyCode.P))
                    FindPanel<PropPanel>().SwitchToggle();

                if (Input.GetKeyDown(KeyCode.K))
                    FindPanel<KnapPanel>().SwitchToggle();

                if (Input.GetKeyDown(KeyCode.E))
                    FindPanel<EquipPanel>().SwitchToggle();
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
            EventManager.Instance.Invoke(EEventType.SceneUnload);
            _cutImage.alpha = 1.0f;
            yield return new WaitForSeconds(1f);
            EventManager.Instance.Invoke(EEventType.SceneLoaded);
            WaitForSeconds sleep = new(0.02f);
            while (_cutImage.alpha > 0)
            {
                _cutImage.alpha -= 0.01f;
                yield return sleep;
            }
        }
    }
}
