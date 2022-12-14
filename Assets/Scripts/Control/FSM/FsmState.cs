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
            return type switch
            {
                FsmStateType.Idle => new Idle(owner, null),
                FsmStateType.Patrol => new Patrol(owner, null, code),
                FsmStateType.Pursuit => new Pursuit(owner, target),
                FsmStateType.Attack => new Attack(owner, target),
                _ => null,
            };
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
    }
}
