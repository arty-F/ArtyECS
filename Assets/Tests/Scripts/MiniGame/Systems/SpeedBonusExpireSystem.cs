using ArtyECS.Core;
using UnityEngine;

public class SpeedBonusExpireSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = World.Query()
            .With<SpeedBonus>()
            .Execute();

        foreach (var entity in entities)
        {
            var speedBonus = entity.GetComponent<SpeedBonus>();
            speedBonus.TimeRamaining -= Time.deltaTime;
            if (speedBonus.TimeRamaining <= 0f)
            {
                entity.RemoveComponent<SpeedBonus>();
            }
        }
    }
}
