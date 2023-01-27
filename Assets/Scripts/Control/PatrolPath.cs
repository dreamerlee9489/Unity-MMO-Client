using System.Collections.Generic;
using UnityEngine;

namespace Control
{
    public class PatrolPath : MonoBehaviour
    {
        private List<Transform> _path = new();

        public int index = 0;
        public List<Transform> Path => _path;

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
                _path.Add(transform.GetChild(i));
        }

        public Vector3 GetCurrPoint() => _path[index].position;
    }
}
