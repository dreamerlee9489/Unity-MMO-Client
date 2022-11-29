using App;
using Control.CMD;
using Control.FSM;
using Frame;
using System.Collections.Generic;
using UnityEngine;

namespace Control
{
    [RequireComponent(typeof(Entity))]
    public class PlayerController : MonoBehaviour
    {
        private RaycastHit _hit;
        private Entity _entity;
        private readonly List<Command> _commands = new List<Command>();

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _commands.Add(new CombatCommand(_entity));
        }

        private void Start()
        {
            foreach (var enemy in GameManager.Instance.ActiveWorld.Enemies)
            {
                if (enemy.CanSee(transform))
                {
                    enemy.ChangeState(new Pursuit(enemy.Entity, _entity));
                }
            }
        }

        private void Update()
        {
            if (gameObject.name == "MainPlayer")
            {
                if (Input.GetMouseButtonDown(1))
                    UndoAll();
                else if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
                    {
                        _entity.Undo();
                        switch (_hit.collider.tag)
                        {
                            case "Terrain":
                                _commands[0].Execute(_hit.point);
                                break;
                        }
                    }
                }

                if (_entity.Agent.velocity != Vector3.zero)
                {
                    foreach (var enemy in GameManager.Instance.ActiveWorld.Enemies)
                    {
                        if (enemy.CanSee(transform))
                        {
                            if (enemy.CurrState is Patrol)
                                enemy.ChangeState(new Pursuit(enemy.Entity, _entity));
                        }
                    }
                }
            }
        }

        public void UndoAll()
        {
            foreach (var command in _commands)
                command.Undo();
        }
    }
}
