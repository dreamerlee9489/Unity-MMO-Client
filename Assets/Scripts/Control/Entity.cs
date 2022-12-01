using Control.CMD;
using Frame;
using Net;
using System.Collections.Generic;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    [RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent))]
    public class Entity : MonoBehaviour
    {
        public static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly float WalkSpeed = 1.56f, RunSpeed = 5.56f;

        private Animator _anim;
        private NavMeshAgent _agent;
        private NameBar _nameBar;

        public Animator Anim => _anim;
        public NavMeshAgent Agent => _agent;
        public NameBar NameBar => _nameBar;

        public int Hp { get; set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _nameBar = transform.Find("NameBar").GetComponent<NameBar>();
            Transform model = transform.Find("Model");
            model.AddComponent<AnimReceiver>();
            _anim = model.GetComponent<Animator>();
        }

        private void Update()
        {
            _anim.SetFloat(MoveSpeed, transform.InverseTransformVector(_agent.velocity).z);
        }
    }
}
