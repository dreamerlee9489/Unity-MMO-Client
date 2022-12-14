using System;
using UnityEngine;

namespace Frame
{
    public class MonoProcessor : MonoBehaviour
    {
        event Action updateEvent = null;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            updateEvent?.Invoke();
        }

        public void AddUpdateAction(Action update)
        {
            updateEvent += update;
        }

        public void RemoveUpdateAction(Action update)
        {
            updateEvent -= update;
        }
    }
}
