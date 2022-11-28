using Control.CMD;
using Net;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent))]
    public class Entity : MonoBehaviour, ICmdReceiver
    {
        private static readonly int _moveSpeed = Animator.StringToHash("moveSpeed");
        public const float WalkSpeed = 1.558401f, RunSpeed = 5.662316f;

        public int hp = 0;
        private Animator _animator;
        private NavMeshAgent _agent;
        private List<Proto.Vector3> _cornerPoints = new();
        private NameBar _nameBar;

        public Animator Animator => _animator;
        public NavMeshAgent Agent => _agent;
        public List<Proto.Vector3> CornerPoints => _cornerPoints;
        public NameBar NameBar => _nameBar;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = transform.Find("Model").GetComponent<Animator>();
            _nameBar = transform.Find("NameBar").GetComponent<NameBar>();
        }

        private void Update()
        {
            if (_cornerPoints.Count > 0)
            {
                Vector3 pos = new Vector3
                {
                    x = _cornerPoints[_cornerPoints.Count - 1].X,
                    y = _cornerPoints[_cornerPoints.Count - 1].Y,
                    z = _cornerPoints[_cornerPoints.Count - 1].Z
                };
                _agent.destination = pos;
                _cornerPoints.Clear();
            }
            _animator.SetFloat(_moveSpeed, transform.InverseTransformVector(_agent.velocity).z);
        }

        public void Execute(Vector3 point)
        {
            Vector3 destPoint = point;
            int layer = 1 << NavMesh.GetAreaFromName("Walkable");
            if (NavMesh.SamplePosition(point, out NavMeshHit meshHit, 10, layer))
                destPoint = meshHit.position;
            NavMeshPath path = new();
            _agent.CalculatePath(destPoint, path);
            if (path.status != NavMeshPathStatus.PathPartial)
            {
                Proto.Move proto = new();
                foreach (Vector3 pos in path.corners)
                    proto.Position.Add(new Proto.Vector3() { X = pos.x, Y = pos.y, Z = pos.z });
                NetManager.Instance.SendPacket(Proto.MsgId.C2SMove, proto);
            }
        }

        public void Execute(Transform transform)
        {
            throw new System.NotImplementedException();
        }

        public void Undo()
        {
            _agent.destination = transform.position + transform.forward;
        }
    }
}
