namespace Control.FSM
{
    public enum StateType { Idle, Patrol, Pursuit, Attack, Death }

    public abstract class State
    {
        protected StateType _type;
        protected FsmController _owner;
        protected PlayerController _target;

        public StateType Type => _type;
        public PlayerController Target => _target;

        protected State(FsmController owner, PlayerController target = null)
        {
            _owner = owner;
            _target = target;
        }

        public static State GenState(StateType type, int code, FsmController owner, PlayerController target)
        {
            return type switch
            {
                StateType.Idle => new Idle(owner, null),
                StateType.Patrol => new Patrol(owner, null, code),
                StateType.Pursuit => new Pursuit(owner, target),
                StateType.Attack => new Attack(owner, target),
                StateType.Death => new Death(owner, null),
                _ => null,
            };
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
    }
}
