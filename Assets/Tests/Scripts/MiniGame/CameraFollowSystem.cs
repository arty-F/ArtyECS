using ArtyECS.Core;
using UnityEngine;

public class CameraFollowSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        if (Camera.main == null)
        {
            return;
        }

        var playerEntities = world.Query().With<Player>().With<Position>().Execute();
        foreach (var player in playerEntities)
        {
            var playerPosition = player.GetComponent<Position>();
            Camera.main.transform.position = new Vector3(
                playerPosition.X,
                Camera.main.transform.position.y,
                playerPosition.Z
            );
            break;
        }
    }
}
