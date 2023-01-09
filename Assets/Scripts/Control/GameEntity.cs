using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(AnimHandler))]
    public abstract class GameEntity : MonoBehaviour
    {
        public static readonly int moveSpeed = Animator.StringToHash("moveSpeed");
        public static readonly int attack = Animator.StringToHash("attack");
        public static readonly int death = Animator.StringToHash("death");
        public static readonly int pickup = Animator.StringToHash("pickup");

        public readonly float walkSpeed = 1.56f;
        public readonly float runSpeed = 5.56f;
        public readonly float attackRadius = 1.5f;
        public readonly float pursuitRadius = 4f;
        public readonly float viewRadius = 6f;

        public Animator anim;
        public NavMeshAgent agent;
        public EntityNameBar nameBar;     

        public ulong Sn { get; set; }

        public int lv = 1, hp = 1000, mp = 1000, atk = 10, def = 0;
        public Transform target;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
            nameBar = transform.Find("EntityNameBar").GetComponent<EntityNameBar>();           
        }

        protected virtual void Update()
        {
            anim.SetFloat(moveSpeed, transform.InverseTransformVector(agent.velocity).z);
        }

        public void SetNameBar(string name)
        {
            nameBar.Name.text = name;
        }

        public bool CanSee(GameEntity target)
        {
            Vector3 direction = target.transform.position - transform.position;
            if (direction.magnitude <= viewRadius)
                return true;
            return false;
        }

        public bool CanAttack(GameEntity target)
        {
            Vector3 direction = target.transform.position - transform.position;
            if (direction.magnitude <= attackRadius)
                return true;
            return false;
        }      
    }
}
