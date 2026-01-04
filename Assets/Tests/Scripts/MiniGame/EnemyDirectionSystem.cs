using ArtyECS.Core;
using UnityEngine;

public class EnemyDirectionSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        Entity playerEntity = null;
        Position playerPosition = default;

        var playerEntities = world.Query().With<Player>().With<Position>().Execute();
        foreach (var player in playerEntities)
        {
            playerEntity = player;
            playerPosition = player.GetComponent<Position>();
            break;
        }

        if (playerEntity == null)
        {
            return;
        }

        var enemies = world.Query().With<Enemy>().With<Position>().With<MovementDirection>().Execute();
        foreach (var enemy in enemies)
        {
            var enemyPosition = enemy.GetComponent<Position>();

            Vector3 direction = new Vector3(
                playerPosition.X - enemyPosition.X,
                0f,
                playerPosition.Z - enemyPosition.Z
            );

            if (direction.magnitude > 0.01f)
            {
                direction.Normalize();
            }

            var movementDirection = enemy.GetComponent<MovementDirection>();
            movementDirection.X = direction.x;
            movementDirection.Y = direction.y;
            movementDirection.Z = direction.z;
            enemy.RemoveComponent(movementDirection);
            enemy.AddComponent(movementDirection);
        }
    }
}
