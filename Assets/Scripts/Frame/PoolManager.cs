using System.Collections.Generic;
using UnityEngine;

namespace Frame
{
    public class PoolType
    {
        public const string RoleToggle = "RoleToggle";
        public const string PatrolPath = "PatrolPath";
    }

    public class PoolManager : MonoSingleton<PoolManager>
    {
        private Dictionary<string, List<GameObject>> _pool = new();
        private Dictionary<string, GameObject> _roots = new();

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnDestroy()
        {
            _pool.Clear();
            _roots.Clear();
        }

        /// <summary>
        /// 实例化count个GameObject并压入池中
        /// </summary>
        public void Add(string poolType, GameObject obj, int count = 10)
        {
            if (!_roots.ContainsKey(poolType))
            {
                GameObject root = new GameObject(poolType + "Root");
                root.transform.parent = transform;
                _roots.Add(poolType, root);
                _pool.Add(poolType, new List<GameObject>());
            }
            for (int i = 0; i < count; i++)
            {
                GameObject instObj = Instantiate(obj);
                instObj.transform.SetParent(_roots[poolType].transform, false);
                instObj.SetActive(false);
                _pool[poolType].Add(instObj);
            }
        }

        /// <summary>
        /// 将一个已实例化的GameObject放入池中
        /// </summary>
        public void Push(string poolType, GameObject instObj)
        {
            if (_roots.ContainsKey(poolType))
            {
                instObj.transform.SetParent(_roots[poolType].transform, false);
                instObj.SetActive(false);
                _pool[poolType].Add(instObj);
            }
        }

        /// <summary>
        /// 将一个已实例化的GameObject弹出池中
        /// </summary>
        public GameObject Pop(string poolType, Transform parent = null)
        {
            if (_pool.ContainsKey(poolType) && _pool[poolType].Count > 0)
            {
                GameObject instObj = _pool[poolType][0];
                instObj.transform.SetParent(parent, false);
                instObj.SetActive(true);
                _pool[poolType].RemoveAt(0);
                return instObj;
            }
            return null;
        }
    }
}
