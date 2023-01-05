using UnityEngine;

namespace Control.CMD
{
    public class TeleportCommand : ICommand
    {
        private readonly Transform _portal;

        public TeleportCommand(IExecutor executor, Transform portal) : base(executor)
        {
            _portal = portal;
        }

        public override void Execute()
        {
            (_executor as ITeleportExecutor).Teleport(_portal);
        }

        public override void Undo()
        {
            (_executor as ITeleportExecutor).UnTeleport();
        }

        public override CommandType GetCommandType() => CommandType.Teleport;
    }
}
