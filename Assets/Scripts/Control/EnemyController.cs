using Control.FSM;
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

        public Entity Entity => _entity;
        public AIState CurrState => _currState;

        public int Id { get; set; }

        private void Awake()
        {
            _entity = GetComponent<Entity>();
        }

        private void Start()
        {
            _currState = new Patrol(_entity);
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
            _entity.Agent.destination = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
        }
    }
}
