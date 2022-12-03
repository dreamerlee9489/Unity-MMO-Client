using UnityEngine;
using UnityEngine.AI;

namespace App
{
    public class TestAgent : MonoBehaviour
    {
        RaycastHit hit;
        NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                print("hit = " + hit.point);
                agent.destination = hit.point;
            }

            print("ramain = " + agent.remainingDistance);
        }
    }
}
