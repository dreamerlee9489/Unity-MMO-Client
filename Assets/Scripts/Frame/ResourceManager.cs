using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Frame
{
    public class ResourceManager : BaseSingleton<ResourceManager>
    {
        public T Load<T>(string path) where T : Object
        {
            T res = Resources.Load<T>(path);
            return res;
        }

        public void LoadAsync<T>(string path, UnityAction<T> func) where T : Object
        {
            MonoManager.Instance.StartCoroutine(RealLoadAsync<T>(path, func));
        }

        IEnumerator RealLoadAsync<T>(string path, UnityAction<T> func) where T : Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;
            func(request.asset as T);
        }
    }
}
