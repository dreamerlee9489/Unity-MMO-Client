namespace Control.FSM
{
    public abstract class AIState
    {
        protected readonly Entity _owner;
        protected readonly Entity _target;

        protected AIState(Entity owner, Entity target = null)
        {
            _owner = owner;
            _target = target;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
    }
}
