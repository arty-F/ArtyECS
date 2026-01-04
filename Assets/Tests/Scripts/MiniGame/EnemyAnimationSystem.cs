using ArtyECS.Core;
using UnityEngine;

public class EnemyAnimationSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var enemies = world.Query().With<Enemy>().With<EnemyExplodingStarted>().Without<EnemyExplodingAnimationStarted>().Execute();
        foreach (var enemy in enemies)
        {
            if (enemy.GameObject != null)
            {
                var animator = enemy.GameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play("EnemyExplode");
                }
                enemy.AddComponent(new EnemyExplodingAnimationStarted());
            }
        }
    }
}