using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

namespace Manage
{
    public class MonoProcessor : MonoBehaviour
    {
        private event Action UpdateEvent = null;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
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

    public class MonoManager : BaseSingleton<MonoManager>
    {
        private readonly MonoProcessor _processor;

        public MonoManager()
        {
            _processor = new GameObject("MonoProcessor").AddComponent<MonoProcessor>();
        }

        public void AddUpdateAction(Action update)
        {
            _processor.AddUpdateAction(update);
        }

        public void RemoveUpdateAction(Action update)
        {
            _processor.RemoveUpdateAction(update);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _processor.StartCoroutine(enumerator);
        }

        public Coroutine StartCoroutine(string methodName)
        {
            return _processor.StartCoroutine(methodName);
        }

        public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
        {
            return _processor.StartCoroutine(methodName, value);
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
            _processor.StopCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine routine)
        {
            _processor.StopCoroutine(routine);
        }

        public void StopCoroutine(string methodName)
        {
            _processor.StopCoroutine(methodName);
        }

        public void StopAllCoroutines()
        {
            _processor.StopAllCoroutines();
        }
    }
}
