using UnityEngine;

namespace Control.CMD
{
	public interface ITeleportExecutor : IExecutor
	{
		void Teleport(Transform portal);
		void UnTeleport();
	}
}
