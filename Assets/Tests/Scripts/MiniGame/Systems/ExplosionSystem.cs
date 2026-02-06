using ArtyECS.Core;
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
            playerPosition = player.Get<Position>();
            playerHealth = player.Get<Health>();
            break;
        }
        var explodingEnemies = world
            .Query()
            .With<Explosion>()
            .Execute();

        /*
         * НА большой скорости пробежал по группе врагов
         * InvalidOperationException: Collection was modified; enumeration operation may not execute.
System.Collections.Generic.List`1+Enumerator[T].MoveNextRare () (at <1eb9db207454431c84a47bcd81e79c37>:0)
System.Collections.Generic.List`1+Enumerator[T].MoveNext () (at <1eb9db207454431c84a47bcd81e79c37>:0)
ExplosionSystem.Execute (ArtyECS.Core.WorldInstance world) (at Assets/Tests/Scripts/MiniGame/Systems/ExplosionSystem.cs:28)
ArtyECS.Core.UpdateProvider.Update () (at Assets/ArtyEcs/Scripts/Core/Common/UpdateProvider.cs:29)

         */

        foreach (var enemy in explodingEnemies)
        {
            var explosion = enemy.Get<Explosion>();
            explosion.TimeRemaining -= Time.deltaTime;

            if (explosion.TimeRemaining > 0f)
            {
                continue;
            }
            var enemyPosition = enemy.Get<Position>();
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

            if (!enemy.Have<Destroying>())
            {
                enemy.Add<Destroying>();
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

                var otherEnemyPosition = otherEnemy.Get<Position>();
                float enemyDistance = Vector3.Distance(explosionPos, new Vector3(otherEnemyPosition.X, otherEnemyPosition.Y, otherEnemyPosition.Z));
                if (enemyDistance <= explosion.ExplosionRadius)
                {
                    otherEnemy.Add<Destroying>();
                }
            }
        }
    }
}
