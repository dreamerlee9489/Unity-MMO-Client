namespace Control.FSM
{
    public enum FsmStateType { Idle, Patrol, Pursuit, Attack }

    public abstract class FsmState
    {
        protected FsmStateType _type;
        protected FsmController _owner;
        protected PlayerController _target;

        public FsmStateType Type => _type;
        public PlayerController Target => _target;

        protected FsmState(FsmController owner, PlayerController target = null)
        {
            _owner = owner;
            _target = target;
        }

        public static FsmState GenState(FsmStateType type, int code, FsmController owner, PlayerController target)
        {
            switch (type)
            {
                case FsmStateType.Idle:
                    return new Idle(owner, null);
                case FsmStateType.Patrol:
                    return new Patrol(owner, null, code);
                case FsmStateType.Pursuit:
                    return new Pursuit(owner, target);
                case FsmStateType.Attack:
                    return new Attack(owner, target);
                default:
                    return null;
            }
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
        public virtual void UpdateState(int code) { }
    }
}
