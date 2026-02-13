using ArtyECS.Core;
using UnityEngine;

public class EnemyNavigateSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var playerEntity = world.GetUniqContext<Player>().Entity;
        var playerPosition = playerEntity.Get<Position>();

        var enemies = world
            .Query()
            .With<Enemy>()
            .With<Position>()
            .With<MoveDirection>()
            .Execute();
        foreach (var enemy in enemies)
        {
            var enemyPosition = enemy.Get<Position>();

            Vector3 direction = new Vector3(playerPosition.X - enemyPosition.X, 0f, playerPosition.Z - enemyPosition.Z);
            direction.Normalize();

            var movementDirection = enemy.Get<MoveDirection>();
            movementDirection.X = direction.x;
            movementDirection.Y = direction.y;
            movementDirection.Z = direction.z;
        }
    }
}
