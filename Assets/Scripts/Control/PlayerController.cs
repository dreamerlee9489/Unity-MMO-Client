using Control.CMD;
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

        private void Update()
        {
            if (gameObject.name != "MainPlayer")
                return;
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                _entity.Undo();
                switch (_hit.collider.tag)
                {
                    case "Terrain":
                        _commands[0].Execute(_hit.point);
                        break;
                }
            }
            else if (Input.GetMouseButtonDown(1))
                UndoAll();
        }

        public void UndoAll()
        {
            foreach (var command in _commands)
                command.Undo();
        }
    }
}
