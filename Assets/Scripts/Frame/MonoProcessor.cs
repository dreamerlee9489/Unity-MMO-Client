using System;
using UnityEngine;

namespace Frame
{
    public class MonoProcessor : MonoBehaviour
    {
        private event Action UpdateEvent = null;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        public void AddUpdateAction(Action update)
        {
            UpdateEvent += update;
        }

        public void RemoveUpdateAction(Action update)
        {
            UpdateEvent -= update;
        }
    }
}
