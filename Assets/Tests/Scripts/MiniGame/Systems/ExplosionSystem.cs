using ArtyECS.Core;
using System.Linq;
using UnityEngine;

public class ExplosionSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        Entity playerEntity = null;
        Position playerPosition = default;
        Health playerHealth = default;

        var playerEntities = world
            .Query()
            .With<Player>()
            .Execute();
        foreach (var player in playerEntities)
        {
            playerEntity = player;
            playerPosition = player.GetComponent<Position>();
            playerHealth = player.GetComponent<Health>();
            break;
        }

        var explodingEnemies = world
            .Query()
            .With<Explosion>()
            .Execute();

        foreach (var enemy in explodingEnemies)
        {
            var explosion = enemy.GetComponent<Explosion>();
            explosion.TimeRemaining -= Time.deltaTime;

            if (explosion.TimeRemaining > 0f)
            {
                continue;
            }
            var enemyPosition = enemy.GetComponent<Position>();
            if (enemyPosition == null)
            {
                continue;
            }
            var explosionPos = new Vector3(enemyPosition.X, enemyPosition.Y, enemyPosition.Z);
            var playerDistance = Vector3.Distance(new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z), explosionPos);
            if (playerDistance <= explosion.ExplosionRadius)
            {
                playerHealth.Amount -= 1f;
                Debug.Log($"Player HP: {playerHealth.Amount}");
            }

            if (!enemy.HasComponent<Destroying>())
            {
                enemy.AddComponent(new Destroying());
            }

            var allEnemies = world
            .Query()
            .With<Enemy>()
            .With<Position>()
            .Without<Destroying>()
            .Execute();
            foreach (var otherEnemy in allEnemies)
            {
                if (otherEnemy.Id == enemy.Id)
                {
                    continue;
                }

                var otherEnemyPosition = otherEnemy.GetComponent<Position>();
                float enemyDistance = Vector3.Distance(explosionPos, new Vector3(otherEnemyPosition.X, otherEnemyPosition.Y, otherEnemyPosition.Z));
                if (enemyDistance <= explosion.ExplosionRadius)
                {
                    otherEnemy.AddComponent(new Destroying());
                }
            }
        }
    }
}
