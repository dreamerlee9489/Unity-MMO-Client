using System.Collections.Generic;

namespace Control.BT
{
    public class Selector : Composite
    {       
        private readonly Dictionary<BtEventId, Action> _nodeDict = new();

        public Selector(BtController npc) : base(npc)
        {
        }

        public override void AddChild(Action child)
        {
            base.AddChild(child);
            switch (child)
            {
                case ActionBirth:
                    _nodeDict.Add(BtEventId.Birth, child);
                    break;
                case ActionDeath:
                    _nodeDict.Add(BtEventId.Death, child);
                    break;
                case ActionIdle:
                    _nodeDict.Add(BtEventId.Idle, child);
                    break;
                case ActionPatrol:
                    _nodeDict.Add(BtEventId.Patrol, child);
                    break;
                case ActionPursue:
                    _nodeDict.Add(BtEventId.Pursue, child);
                    break;
                case ActionAttack:
                    _nodeDict.Add(BtEventId.Attack, child);
                    break;
                case ActionFlee:
                    _nodeDict.Add(BtEventId.Flee, child);
                    break;
                default:
                    break;
            }
        }

        public override void RemoveChild(Action child)
        {
            base.RemoveChild(child);
            foreach (var pair in _nodeDict)
            {
                if (child == pair.Value) 
                {
                    _nodeDict.Remove(pair.Key);
                    break;
                }
            }
        }

        protected override void Enter()
        {
        }

        protected override BtStatus Execute()
        {
            return status = curNode == null ? BtStatus.Running : curNode.Tick();
        }

        protected override void Exit()
        {
        }

        public void SwitchNode(BtEventId id)
        {
            curNode?.ForceExit(BtStatus.Invalid);
            curNode = _nodeDict[id];
        }

        public void SyncAction(BtEventId id)
        {
            curNode?.ForceExit(BtStatus.Suspend);
            curNode = _nodeDict[id];
        }
    }
}
