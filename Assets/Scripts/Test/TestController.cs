using UnityEngine;
using UnityEngine.AI;

namespace App
{
    public class TestController : MonoBehaviour
    {
        RaycastHit hit;
        NavMeshAgent agent;
        Vector3 initPos = new Vector3(0, 0, 0);
        Vector3 testPos1 = new Vector3(10, 0, 10);
        Vector3 testPos2 = new Vector3(-10, 0, -10);

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                NavMeshPath path = new();
                agent.CalculatePath(testPos1, path);
                if (path.status != NavMeshPathStatus.PathPartial)
                {
                    print("Q len=" + path.corners.Length);
                    agent.destination = testPos1;
                    foreach (Vector3 pos in path.corners)
                    {
                        print(pos);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                NavMeshPath path = new();
                agent.CalculatePath(initPos, path);
                if (path.status != NavMeshPathStatus.PathPartial)
                {
                    print("W len=" + path.corners.Length);
                    agent.destination = initPos;
                    foreach (Vector3 pos in path.corners)
                    {
                        print(pos);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                NavMeshPath path = new();
                agent.CalculatePath(testPos2, path);
                if (path.status != NavMeshPathStatus.PathPartial)
                {
                    print("E len=" + path.corners.Length);
                    agent.destination = testPos2;
                    foreach (Vector3 pos in path.corners)
                    {
                        print(pos);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                agent.destination = testPos1;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                agent.destination = testPos2;
            }

            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                print("hit=" + hit.point);
                agent.destination = hit.point;
            }
        }
    }
}
