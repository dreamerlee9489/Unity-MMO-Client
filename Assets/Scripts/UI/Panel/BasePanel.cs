using Manage;
using UnityEngine;

namespace UI
{
    public class BasePanel : MonoBehaviour
    {
        protected string _panelType;
        protected static UIManager _canvas;

        public virtual void Open() => gameObject.SetActive(true);

        public virtual void Close() => gameObject.SetActive(false);

        /// <summary>
        /// 必须在子类Awake()中赋值_panelType并获取组件
        /// </summary>
        protected virtual void Awake()
        {
            if (_canvas == null)
                _canvas = GameObject.Find("UIManager").GetComponent<UIManager>();
            _panelType = GetType().Name;
            _canvas.AddPanel(_panelType, this);
        }
    }
}
