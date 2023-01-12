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

        public Transform tempSlot, hudGroup;

        protected override void Awake()
        {
            base.Awake();
            _cutImage = transform.Find("CutImage").GetComponent<CanvasGroup>();
            tempSlot = transform.Find("TempSlot");
            hudGroup = transform.Find("HUDGroup");
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void Update()
        {
            if(GameManager.currWorld)
            {
                if (Input.GetKeyDown(KeyCode.P))
                    GetPanel<PropPanel>().SwitchToggle();

                if (Input.GetKeyDown(KeyCode.K))
                    GetPanel<KnapPanel>().SwitchToggle();

                if (Input.GetKeyDown(KeyCode.E))
                    GetPanel<EquipPanel>().SwitchToggle();
            }
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void DisconnectCallback(EAppType appType)
        {
        }

        public T GetPanel<T>() where T : BasePanel
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
            _cutImage.alpha = 1f;
            yield return new WaitForSeconds(1f);
            EventManager.Instance.Invoke(EEventType.PlayerLoaded);
            yield return new WaitForSeconds(1f);
            _cutImage.alpha = 0f;
        }
    }
}
