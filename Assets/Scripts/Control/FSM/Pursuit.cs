using Frame;
using System.Collections;
using UnityEngine;

namespace Control.FSM
{
    public class Pursuit : AIState
    {
        WaitForSeconds _wait1s = new WaitForSeconds(1);

        public Pursuit(Entity owner, Entity target) : base(owner, target)
        {
        }

        public override void Enter()
        {
            _owner.Agent.speed = Entity.RunSpeed;
            MonoManager.Instance.StartCoroutine(UpdatePos());
        }

        public override void Execute()
        {
            _owner.Agent.destination = _target.transform.position;
            //_owner.Execute(_target.transform.position);
        }

        public override void Exit()
        {
            _owner.Agent.speed = Entity.WalkSpeed;
            MonoManager.Instance.StopCoroutine(UpdatePos());
            Debug.Log("PURSUIT EXIT");
        }

        private IEnumerator UpdatePos()
        {
            while (true)
            {
                _owner.Execute(_target.transform.position);
                yield return _wait1s;
            }
        }
    }
}
