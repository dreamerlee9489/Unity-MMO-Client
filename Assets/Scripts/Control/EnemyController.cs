using Control.FSM;
using Frame;
using UnityEngine;

namespace Control
{
    [RequireComponent(typeof(Entity))]
    public class EnemyController : MonoBehaviour
    {
        public static readonly float SqrViewRadius = 6;

        private Entity _entity;
        private AIState _prevState;
        private AIState _currState;
        private PatrolPath _patrolPath;

        public Entity Entity => _entity;
        public AIState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;

        public int Id { get; set; }

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
        }

        private void Start()
        {
            _patrolPath.transform.position = transform.position;
            _currState = new Idle(_entity);
        }

        private void Update()
        {
            if (_currState != null)
                _currState.Execute();
        }

        public void ChangeState(AIState newState)
        {
            _prevState = _currState;
            _currState.Exit();
            _currState = newState;
            _currState.Enter();
        }

        public bool CanSee(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            if (direction.sqrMagnitude <= SqrViewRadius)
                return true;
            return false;
        }

        public void ParseProto(Proto.Enemy proto)
        {
            _entity.Agent.enabled = false;
            _entity.transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            _entity.Agent.enabled = true;
        }
    }
}
