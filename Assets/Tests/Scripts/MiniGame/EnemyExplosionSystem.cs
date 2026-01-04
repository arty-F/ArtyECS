using ArtyECS.Core;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosionSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        Entity playerEntity = null;
        Position playerPosition = default;
        Health playerHealth = default;

        var playerEntities = world.Query().With<Player>().With<Position>().With<Health>().Execute();
        foreach (var player in playerEntities)
        {
            playerEntity = player;
            playerPosition = player.GetComponent<Position>();
            playerHealth = player.GetComponent<Health>();
            break;
        }

        if (playerEntity == null)
        {
            return;
        }

        var enemiesToDestroy = new List<Entity>();

        var explodingEnemies = world.Query().With<EnemyExplodingStarted>().With<EnemyExplosion>().With<Position>().Execute();
        foreach (var enemy in explodingEnemies)
        {
            var explosion = enemy.GetComponent<EnemyExplosion>();
            var enemyPosition = enemy.GetComponent<Position>();

            explosion.TimeRemaining -= Time.deltaTime;

            if (explosion.TimeRemaining <= 0f)
            {
                Vector3 explosionPos = new Vector3(enemyPosition.X, enemyPosition.Y, enemyPosition.Z);

                float playerDistance = Vector3.Distance(
                    new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z),
                    explosionPos
                );

                if (playerDistance <= explosion.ExplosionRadius)
                {
                    playerHealth.Amount -= 1f;
                    playerEntity.RemoveComponent(playerHealth);
                    playerEntity.AddComponent(playerHealth);
                    Debug.Log($"Player HP: {playerHealth.Amount}");
                }

                var allEnemies = world.Query().With<Enemy>().With<Position>().Execute();
                foreach (var otherEnemy in allEnemies)
                {
                    if (otherEnemy.Id == enemy.Id)
                    {
                        continue;
                    }

                    var otherEnemyPosition = otherEnemy.GetComponent<Position>();
                    float enemyDistance = Vector3.Distance(
                        explosionPos,
                        new Vector3(otherEnemyPosition.X, otherEnemyPosition.Y, otherEnemyPosition.Z)
                    );

                    if (enemyDistance <= explosion.ExplosionRadius)
                    {
                        enemiesToDestroy.Add(otherEnemy);
                    }
                }

                enemiesToDestroy.Add(enemy);
            }
            else
            {
                enemy.RemoveComponent(explosion);
                enemy.AddComponent(explosion);
            }
        }

        foreach (var enemyToDestroy in enemiesToDestroy)
        {
            world.DestroyEntity(enemyToDestroy);
        }
    }
}
