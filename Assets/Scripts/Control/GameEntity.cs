using Manage;
using System;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent), typeof(AnimHandler))]
    public abstract class GameEntity : MonoBehaviour
    {
        public static readonly int moveSpeed = Animator.StringToHash("moveSpeed");
        public static readonly int attack = Animator.StringToHash("attack");
        public static readonly int death = Animator.StringToHash("death");
        public static readonly int pickup = Animator.StringToHash("pickup");

        protected int _hashCode = 0;
        protected static WorldManager _currWorld;
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

        public Transform target;

        public int lv = 1, hp = 1000, mp = 1000, atk = 10, def = 0;

        public override int GetHashCode() => _hashCode;

        public override bool Equals(object other) => _hashCode == other.GetHashCode();

        protected virtual void Awake()
        {
            _hashCode = Guid.NewGuid().GetHashCode();
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _nameBar = transform.Find("EntityNameBar").GetComponent<EntityNameBar>();
            EventManager.Instance.AddListener(EEventType.SceneUnload, SceneUnloadCallback);
            EventManager.Instance.AddListener(EEventType.SceneLoaded, SceneLoadedCallback);
        }

        protected virtual void Update()
        {
            _anim.SetFloat(moveSpeed, transform.InverseTransformVector(_agent.velocity).z);
        }

        protected virtual void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener(EEventType.SceneUnload, SceneUnloadCallback);
            EventManager.Instance.RemoveListener(EEventType.SceneLoaded, SceneLoadedCallback);
        }

        protected void SceneUnloadCallback()
        {
            if(_currWorld)
                _currWorld.inWorldObjDict.Clear();
            _currWorld = null;
        }

        protected void SceneLoadedCallback()
        {
            if (_currWorld == null)
                _currWorld = FindObjectOfType<WorldManager>();
            _currWorld.inWorldObjDict.Add(_hashCode, transform);
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
