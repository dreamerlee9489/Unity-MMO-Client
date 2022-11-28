using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Frame
{
    public class MonoManager : BaseSingleton<MonoManager>
    {
        MonoProcessor processor;

        public MonoManager()
        {
            processor = new GameObject("MonoProcessor").AddComponent<MonoProcessor>();
        }

        public void AddUpdateAction(Action update)
        {
            processor.AddUpdateAction(update);
        }

        public void RemoveUpdateAction(Action update)
        {
            processor.RemoveUpdateAction(update);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return processor.StartCoroutine(enumerator);
        }

        public Coroutine StartCoroutine(string methodName)
        {
            return processor.StartCoroutine(methodName);
        }

        public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
        {
            return processor.StartCoroutine(methodName, value);
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
            processor.StopCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine routine)
        {
            processor.StopCoroutine(routine);
        }

        public void StopCoroutine(string methodName)
        {
            processor.StopCoroutine(methodName);
        }

        public void StopAllCoroutines()
        {
            processor.StopAllCoroutines();
        }
    }
}
