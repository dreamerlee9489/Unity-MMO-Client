using Control.BT;
using UnityEngine;

namespace App
{
    public class TestSwitch : MonoBehaviour
	{
		Node node;

        private void Start()
        {
            node = new ActionAttack(null);
            switch (node)
            {
                case ActionBirth:
                    break;
                case ActionDeath:
                    break;
                case ActionIdle:
                    break;
                case ActionPatrol: 
                    break;
                case ActionPursue: 
                    break;
                case ActionAttack:
                    Debug.Log("node is ActionAttack");
                    break;
                case Node _:
                    break;
            }
        }
    }
}
