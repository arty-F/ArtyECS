using ArtyECS.Core;
using UnityEngine;

public class ProximitySystem : SystemHandler
{
    private const string ANIMATION_NAME = "EnemyExplode";
    private readonly float _explodeTime;
    private readonly float _explodeRadius;

    public ProximitySystem(float explodeTime, float explodeRadius)
    {
        _explodeTime = explodeTime;
        _explodeRadius = explodeRadius;
    }

    public override void Execute(WorldInstance world)
    {
        Position playerPosition = default;
        var playerEntities = world
            .Query()
            .With<Player>()
            .Execute();
        foreach (var player in playerEntities)
        {
            playerPosition = player.Get<Position>();
            break;
        }

        var enemies = world
            .Query()
            .With<ProximityBomb>()
            .With<Position>()
            .Without<Explosion>()
            .Execute();
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
                explosion.TimeRemaining = _explodeTime;
                explosion.ExplosionRadius = _explodeRadius;
                var animator = enemy.GameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play(ANIMATION_NAME);
                }
            }
        }
    }
}
