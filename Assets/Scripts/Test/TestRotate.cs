using UnityEngine;

namespace App
{
    public class TestRotate : MonoBehaviour
    {
        private void Start()
        {
            transform.position = new Vector3(0, 8, -10);
            transform.rotation = Quaternion.AngleAxis(-35, Vector3.left);
        }
    }
}
