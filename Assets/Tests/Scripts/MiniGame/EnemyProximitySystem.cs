using ArtyECS.Core;
using UnityEngine;

public class EnemyProximitySystem : SystemHandler
{
    private float _proximityDistance;
    private float _enemyExplosionTime;
    private float _enemyExplosionRadius;

    public EnemyProximitySystem(float proximityDistance, float enemyExplosionTime, float enemyExplosionRadius)
    {
        _proximityDistance = proximityDistance;
        _enemyExplosionTime = enemyExplosionTime;
        _enemyExplosionRadius = enemyExplosionRadius;
    }

    public override void Execute(WorldInstance world)
    {
        Position playerPosition = default;

        var playerEntities = world.Query().With<Player>().With<Position>().Execute();
        foreach (var player in playerEntities)
        {
            playerPosition = player.GetComponent<Position>();
            break;
        }

        var enemies = world.Query().With<Enemy>().With<Position>().With<MovementDirection>().Without<EnemyExplodingStarted>().Execute();
        foreach (var enemy in enemies)
        {
            var enemyPosition = enemy.GetComponent<Position>();

            float distance = Vector3.Distance(
                new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z),
                new Vector3(enemyPosition.X, enemyPosition.Y, enemyPosition.Z)
            );

            if (distance <= _proximityDistance)
            {
                enemy.AddComponent(new EnemyExplodingStarted());
                var enemyMove = enemy.GetComponent<MovementDirection>();
                enemy.RemoveComponent(enemyMove);
                enemy.AddComponent(new EnemyExplosion
                {
                    TimeRemaining = _enemyExplosionTime,
                    ExplosionRadius = _enemyExplosionRadius
                });
            }
        }
    }
}
