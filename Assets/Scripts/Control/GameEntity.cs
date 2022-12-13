using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent), typeof(AnimExecutor))]
    public abstract class GameEntity : MonoBehaviour
    {
        public static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public static readonly int Attack = Animator.StringToHash("Attack");

        protected float _walkSpeed = 1.56f;
        protected float _runSpeed = 5.56f;
        protected float _attackRadius = 1.5f;
        protected float _pursuitRadius = 4f;
        protected float _viewRadius = 6f;
        protected Animator _anim;
        protected NavMeshAgent _agent;
        protected NameBar _nameBar;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float AttackRadius => _attackRadius;
        public float PursuitRadius => _pursuitRadius;
        public float ViewRadius => _viewRadius;
        public Animator Anim => _anim;
        public NavMeshAgent Agent => _agent;
        public NameBar NameBar => _nameBar;

        public int Hp { get; set; }

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _nameBar = transform.Find("NameBar").GetComponent<NameBar>();
        }

        protected virtual void Update()
        {
            _anim.SetFloat(MoveSpeed, transform.InverseTransformVector(_agent.velocity).z);
        }

        public bool CanSee(GameEntity target)
        {
            if (target)
            {
                Vector3 direction = target.transform.position - transform.position;
                if (direction.magnitude <= ViewRadius)
                    return true;
            }
            return false;
        }

        public bool CanAttack(GameEntity target)
        {
            if (target)
            {
                Vector3 direction = target.transform.position - transform.position;
                if (direction.magnitude <= AttackRadius)
                    return true;
            }
            return false;
        }
    }
}
