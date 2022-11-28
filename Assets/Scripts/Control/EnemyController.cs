using UnityEngine;

namespace Control
{
    [RequireComponent(typeof(Entity))]
    public class EnemyController : MonoBehaviour
    {
        private Entity _entity;

        public Entity Entity => _entity;

        private void Awake()
        {
            _entity = GetComponent<Entity>();
        }

        private void Start()
        {
            _entity.Agent.speed = Entity.WalkSpeed;
        }

        public void Parse(Proto.Enemy proto)
        {
            _entity.hp = proto.Hp;
            _entity.Agent.destination = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            Debug.Log("Parse: " + _entity.hp + " " + _entity.Agent.destination);
        }
    }
}
