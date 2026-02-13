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

        var player = world.GetUniqContext<Player>().Entity;
        var playerPosition = player.Get<Position>();

        Camera.main.transform.position = new Vector3(
            playerPosition.X,
            Camera.main.transform.position.y,
            playerPosition.Z
        );
    }
}
