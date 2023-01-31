using System.Collections.Generic;
using UnityEngine;

namespace Control
{
    public class PatrolPath : MonoBehaviour
    {
        private List<Transform> _path = new();

        public int index = 0;
        public Vector3 CurrPoint => _path[index].position;
        public List<Transform> Path => _path;

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
                _path.Add(transform.GetChild(i));
        }
    }
}
