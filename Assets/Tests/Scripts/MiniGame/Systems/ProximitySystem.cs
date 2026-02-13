using ArtyECS.Core;
using UnityEngine;

public class ProximitySystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var playerEntity = world.GetUniqContext<Player>().Entity;
        var playerPosition = playerEntity.Get<Position>();

        var enemies = world
            .Query()
            .With<ProximityBomb>()
            .With<Position>()
            .Without<Explosion>()
            .Execute();

        var enemySpawnConfig = world.GetUniqContext<EnemySpawnConfig>();

        foreach (var enemy in enemies)
        {
            var enemyPosition = enemy.Get<Position>();
            var distance = Vector3.Distance(new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z),
                new Vector3(enemyPosition.X, enemyPosition.Y, enemyPosition.Z));
            var triggerDistance = enemy.Get<ProximityBomb>().TriggerDistance;

            if (distance <= triggerDistance)
            {
                enemy.Remove<MoveDirection>();
                var explosion = enemy.Add<Explosion>();
                explosion.TimeRemaining = enemySpawnConfig.ExplodeTime;
                explosion.ExplosionRadius = enemySpawnConfig.ExplodeRadius;
            }
        }
    }
}
