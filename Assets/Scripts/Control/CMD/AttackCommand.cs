namespace Control.CMD
{
    public interface IAttacker : IExecutor
    {
        void Attack(BtController target);
        void UnAttack();
    }

    public class AttackCommand : ICommand
    {
        private BtController _target;

        public AttackCommand(IAttacker executor, BtController target) : base(executor)
        {
            _target = target;
            (_executor as GameEntity).Target = target.transform;
            (_executor as IAttacker).Attack(_target);
        }

        public override CommandType GetCommandType() => CommandType.Attack;

        public override void Execute()
        {
            (_executor as IAttacker).Attack(_target);
        }

        public override void Undo()
        {
            base.Undo();
            (_executor as IAttacker).UnAttack();
        }
    }
}
