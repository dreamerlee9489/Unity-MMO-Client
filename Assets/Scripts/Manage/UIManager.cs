using System.Collections.Generic;
using UI;
using UnityEngine.UI;

namespace Manage
{
    public class UIManager : MonoSingleton<UIManager>
    {
        private Text _debug;
        private readonly Dictionary<string, BasePanel> _panelDict = new();

        protected override void Awake()
        {
            base.Awake();
            _debug = transform.Find("Debug").GetComponent<Text>();
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
    }
}
