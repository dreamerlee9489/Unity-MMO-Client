using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Manage
{
    public class UIManager : MonoSingleton<UIManager>
    {
        private Text _debug;
        private CanvasGroup _image;
        private readonly Dictionary<string, BasePanel> _panelDict = new();

        protected override void Awake()
        {
            base.Awake();
            _debug = transform.Find("Debug").GetComponent<Text>();
            _image = transform.Find("Image").GetComponent<CanvasGroup>();
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
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

        public void DebugLog(string log)
        {
            _debug.text = log;
        }

        public IEnumerator FadeAlpha()
        {
            _image.alpha = 1.0f;
            yield return new WaitForSeconds(1f);
            WaitForSeconds sleep = new(0.02f);
            while (_image.alpha > 0)
            {
                _image.alpha -= 0.01f;
                yield return sleep;
            }
        }
    }
}
