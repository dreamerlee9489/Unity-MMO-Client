using UnityEngine;

namespace Control.CMD
{
    public interface ITeleporter : IExecutor
    {
        void Teleport(Transform portal);
        void UnTeleport();
    }

    public class TeleportCommand : ICommand
    {
        private readonly Transform _portal;

        public TeleportCommand(IExecutor executor, Transform portal) : base(executor)
        {
            _portal = portal;
        }

        public override void Execute()
        {
            (_executor as ITeleporter).Teleport(_portal);
        }

        public override void Undo()
        {
            (_executor as ITeleporter).UnTeleport();
        }

        public override CommandType GetCommandType() => CommandType.Teleport;
    }
}
