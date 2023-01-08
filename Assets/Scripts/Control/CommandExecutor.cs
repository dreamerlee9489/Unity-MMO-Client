using Control.CMD;
using Items;
using UnityEngine;

namespace Control
{
    public partial class PlayerController : IMoveExecutor, IAttackExecutor, IPickupExecutor, ITeleportExecutor
    {
        public void Move(Vector3 point)
        {
            _agent.destination = point;
            _cmd = null;
        }

        public void UnMove()
        {
        }

        public void Attack(Transform target)
        {
            transform.LookAt(target);
            if (Vector3.Distance(transform.position, target.position) <= AttackRadius)
                _anim.SetBool(attack, true);
            else
            {
                _anim.SetBool(attack, false);
                _agent.destination = target.position;
            }
        }

        public void UnAttack()
        {
            _anim.SetBool(attack, false);
            _cmd = null;
        }

        public void Pickup(Transform item)
        {
            _agent.destination = item.position;
            if (Vector3.Distance(transform.position, item.position) <= _agent.stoppingDistance)
            {
                _anim.SetTrigger(pickup);
                _cmd = null;
            }
        }

        public void UnPickup()
        {
        }

        public void Teleport(Transform portal)
        {
            _agent.destination = portal.position;
            if (Vector3.Distance(transform.position, portal.position) <= _agent.stoppingDistance)
            {
                portal.GetComponent<Portal>().OpenDoor(this);
                _cmd = null;
            }
        }

        public void UnTeleport()
        {
        }
    }
}
