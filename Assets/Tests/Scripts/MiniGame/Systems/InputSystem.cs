using ArtyECS.Core;
using UnityEngine;

public class InputSystem : SystemHandler
{
    private Vector3 _cached = Vector3.zero;

    public override void Execute(WorldInstance world)
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveZ = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveZ = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
        }

        _cached.x = moveX;
        _cached.z = moveZ;
        _cached.Normalize();

        var players = world.Query().With<Player>().With<MoveDirection>().Execute();
        foreach (var player in players)
        {
            var movementDirection = player.Get<MoveDirection>();
            movementDirection.X = _cached.x;
            movementDirection.Y = _cached.y;
            movementDirection.Z = _cached.z;
        }
    }
}
