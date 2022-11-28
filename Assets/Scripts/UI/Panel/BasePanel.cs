using Frame;
using UnityEngine;

namespace UI
{
    public class PanelType
    {
        public const string UICanvas = "UICanvas";
        public const string StartPanel = "StartPanel";
        public const string LoginPanel = "LoginPanel";
        public const string RolesPanel = "RolesPanel";
        public const string CreatePanel = "CreatePanel";
        public const string ModalPanel = "ModalPanel";
    }

    public class BasePanel : MonoBehaviour
    {
        protected string _panelType = "UICanvas";
        protected static UICanvas _canvas = null;

        public virtual void Open() => gameObject.SetActive(true);

        public virtual void Close() => gameObject.SetActive(false);

        /// <summary>
        /// 必须在子类Awake()中赋值_panelType并获取组件
        /// </summary>
        virtual protected void Awake()
        {
            if (_canvas == null)
                _canvas = GameObject.Find("UICanvas").GetComponent<UICanvas>();
        }

        virtual protected void Start()
        {
            if (_panelType != "UICanvas")
                _canvas.AddPanel(_panelType, this);
        }
    }
}
