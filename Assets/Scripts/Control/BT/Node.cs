namespace Control.BT
{
    public enum BtEventId { Unknow, Birth, Alive, Death, Idle, Patrol, Pursue, Attack, Flee };
    public enum BtStatus { Invalid, Running, Success, Failure, Suspend, Aborted };

    public abstract class Node
	{
        public BtStatus status = BtStatus.Invalid;
        public BtController npc = null;

        public Node(BtController npc) { this.npc = npc; }

        protected abstract void Enter();
        protected abstract BtStatus Execute();
        protected abstract void Exit();

        public BtStatus Tick()
        {
            if (status != BtStatus.Running)
                Enter();
            status = Execute();
            if (status != BtStatus.Running)
                Exit();
            return status;
        }

        public void ForceExit(BtStatus status)
        {
            this.status = status;
            Exit();
        }
    }
}
