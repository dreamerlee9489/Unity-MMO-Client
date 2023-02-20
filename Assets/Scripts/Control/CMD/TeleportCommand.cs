using Items;

namespace Control.CMD
{
    public interface ITeleporter : IExecutor
    {
        void Teleport(GameItem portal);
        void UnTeleport();
    }

    public class TeleportCommand : ICommand
    {
        private readonly GameItem _portal;

        public TeleportCommand(IExecutor executor, GameItem portal) : base(executor)
        {
            _portal = portal;
            (_executor as GameEntity).Target = portal.transform;
            (_executor as ITeleporter).Teleport(_portal);
        }

        public override CommandType GetCmdType() => CommandType.Teleport;

        public override void Execute()
        {
            (_executor as ITeleporter).Teleport(_portal);
        }

        public override void Undo()
        {
            (_executor as ITeleporter).UnTeleport();
        }
    }
}
