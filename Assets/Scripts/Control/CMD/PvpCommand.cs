namespace Control.CMD
{
    public interface IFighter : IExecutor
    {
        void Pvp(PlayerController target);
        void UnPvp();
    }

    public class PvpCommand : ICommand
    {
        private PlayerController _target;

        public PvpCommand(IExecutor executor, PlayerController target) : base(executor)
        {
            _target = target;
            (_executor as GameEntity).Target = target.transform;
            (_executor as IFighter).Pvp(_target);
        }

        public override CommandType GetCmdType() => CommandType.Pvp;

        public override void Execute()
        {
            (_executor as IFighter).Pvp(_target);
        }

        public override void Undo()
        {
            (_executor as IFighter).UnPvp();
        }
    }
}
