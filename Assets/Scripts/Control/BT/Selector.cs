using System.Collections.Generic;

namespace Control.BT
{
    public class Selector : Composite
    {       
        private Dictionary<BtEventId, Node> _nodeDict = new();

        public Selector(BtController npc) : base(npc)
        {
        }

        public override void AddChild(Node child)
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
                default:
                    break;
            }
        }

        public override void RemoveChild(Node child)
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
            //foreach (var pair in _nodeDict) 
            //    if (pair.Value == curr)
            //        Debug.Log("curr = " + pair.Key);
            return status = curr == null ? BtStatus.Running : curr.Tick();
        }

        protected override void Exit()
        {
        }

        public void SyncAction(int id)
        {
            curr?.ForceExit(BtStatus.Suspend);
            curr = _nodeDict[(BtEventId)id];
        }
    }
}
