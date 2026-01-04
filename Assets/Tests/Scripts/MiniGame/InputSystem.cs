using ArtyECS.Core;
using UnityEngine;

public class InputSystem : SystemHandler
{
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

        Vector3 direction = new Vector3(moveX, 0f, moveZ);
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        var players = world.Query().With<Player>().With<MovementDirection>().Execute();
        foreach (var player in players)
        {
            var movementDirection = player.GetComponent<MovementDirection>();
            movementDirection.X = direction.x;
            movementDirection.Y = direction.y;
            movementDirection.Z = direction.z;
            player.RemoveComponent(movementDirection);
            player.AddComponent(movementDirection);
        }
    }
}
