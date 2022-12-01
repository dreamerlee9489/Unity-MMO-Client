using App;

namespace Control.FSM
{
    public enum AIStateType
    {
        Idle, Patrol, Pursuit, Attack
    }

    public abstract class AIState
    {
        protected readonly Entity _owner;
        protected readonly Entity _target;

        public AIStateType type;
        public Entity Target => _target;

        protected AIState(Entity owner, Entity target = null)
        {
            _owner = owner;
            _target = target;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
        public abstract void UpdateState(int code);

        public static AIState GenState(AIStateType type, Entity owner, Entity target)
        {
            switch (type)
            {
                case AIStateType.Idle:
                    return new Idle(owner, null);
                case AIStateType.Patrol:
                    return new Patrol(owner, null);
                case AIStateType.Pursuit:
                    return new Pursuit(owner, target);
                case AIStateType.Attack:
                    return new Attack(owner, target);
                default:
                    return null;
            }
        }
    }
}
