using Manage;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class UICanvas : BasePanel
    {
        private readonly Dictionary<string, BasePanel> _panelDict = new();
        private readonly Text _debug;

        protected override void Awake()
        {
            DontDestroyOnLoad(this);
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
            string type = typeof(T).Name;
            if (_panelDict.ContainsKey(type))
                return _panelDict[type].GetComponent<T>();
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
