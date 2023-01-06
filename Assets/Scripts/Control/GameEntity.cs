using Manage;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent), typeof(AnimHandler))]
    public abstract class GameEntity : GuidObject
    {
        public static readonly int moveSpeed = Animator.StringToHash("moveSpeed");
        public static readonly int attack = Animator.StringToHash("attack");
        public static readonly int death = Animator.StringToHash("death");
        public static readonly int pickup = Animator.StringToHash("pickup");

        protected readonly float _walkSpeed = 1.56f;
        protected readonly float _runSpeed = 5.56f;
        protected readonly float _attackRadius = 1.5f;
        protected readonly float _pursuitRadius = 4f;
        protected readonly float _viewRadius = 6f;
        protected Animator _anim;
        protected NavMeshAgent _agent;
        protected EntityNameBar _nameBar;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float AttackRadius => _attackRadius;
        public float PursuitRadius => _pursuitRadius;
        public float ViewRadius => _viewRadius;
        public Animator Anim => _anim;
        public NavMeshAgent Agent => _agent;

        public int lv = 1, hp = 1000, mp = 1000, atk = 10, def = 0;
        public Transform target;

        protected override void Awake()
        {
            base.Awake();
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _nameBar = transform.Find("EntityNameBar").GetComponent<EntityNameBar>();
        }

        protected virtual void Update()
        {
            _anim.SetFloat(moveSpeed, transform.InverseTransformVector(_agent.velocity).z);
        }

        public void SetNameBar(string name)
        {
            _nameBar.Name.text = name;
        }

        public bool CanSee(GameEntity target)
        {
            Vector3 direction = target.transform.position - transform.position;
            if (direction.magnitude <= ViewRadius)
                return true;
            return false;
        }

        public bool CanAttack(GameEntity target)
        {
            Vector3 direction = target.transform.position - transform.position;
            if (direction.magnitude <= AttackRadius)
                return true;
            return false;
        }

        public void SetHp(GameEntity attacker, int currHp)
        {
            if ((hp = currHp) == 0)
            {
                attacker.target = null;
                switch (attacker)
                {
                    case PlayerController:
                        (attacker as PlayerController).ResetCmd();
                        break;
                    case FsmController:
                        (attacker as FsmController).ResetState();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
