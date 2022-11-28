using Frame;
using Net;
using System.Collections.Generic;

namespace UI
{
    public class UICanvas : BasePanel
    {
        private Dictionary<string, BasePanel> panelDict = new();

        protected override void Awake()
        {
            DontDestroyOnLoad(this);
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        public T GetPanel<T>() where T : BasePanel
        {
            string type = typeof(T).Name;
            if (panelDict.ContainsKey(type))
                return panelDict[type].GetComponent<T>();
            return null;
        }

        public void AddPanel(string type, BasePanel panel)
        {
            if (!panelDict.ContainsKey(type))
                panelDict.Add(type, panel);
        }

        private void DisconnectCallback(EAppType appType)
        {
            //print("UICanvas.DisconnectCallback apptype=" + appType);
        }
    }
}
