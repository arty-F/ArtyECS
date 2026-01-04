using ArtyECS.Core;
using UnityEngine;

public class SpeedBonusSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<SpeedBonus>().Execute();
        foreach (var entity in entities)
        {
            var speedBonus = entity.GetComponent<SpeedBonus>();
            speedBonus.RemainingTime -= Time.deltaTime;

            if (speedBonus.RemainingTime <= 0f)
            {
                entity.RemoveComponent(speedBonus);
            }
            else
            {
                entity.RemoveComponent(speedBonus);
                entity.AddComponent(speedBonus);
            }
        }
    }
}

