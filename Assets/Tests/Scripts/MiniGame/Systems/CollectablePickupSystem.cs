using ArtyECS.Core;
using UnityEngine;

public class CollectablePickupSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var collectablePickupers = World.Query()
            .With<CollectablePickuper>()
            .Execute();

        var collectables = World.Query()
            .With<Collectable>()
            .Without<Destroying>()
            .Execute();

        foreach (var collectable in collectables)
        {
            if (collectable.GameObject == null)
            {
                continue;
            }

            foreach (var pickuper in collectablePickupers)
            {
                if (pickuper.GameObject == null)
                {
                    continue;
                }

                var pickupComponent = pickuper.Get<CollectablePickuper>();

                if (Vector3.Distance(collectable.GameObject.transform.position, pickuper.GameObject.transform.position) < pickupComponent.PickupRange)
                {
                    collectable.Add<Destroying>();
                    var collectableComponent = collectable.Get<Collectable>();
                    if (!pickuper.Have<SpeedBonus>())
                    {
                        var speedBonus = pickuper.Add<SpeedBonus>();
                        speedBonus.Value = collectableComponent.SpeedBonus;
                        speedBonus.TimeRamaining = collectableComponent.BonusDuration;
                    }
                    else
                    {
                        var speedBonus = pickuper.Get<SpeedBonus>();
                        speedBonus.Value += collectableComponent.SpeedBonus;
                        speedBonus.TimeRamaining = collectableComponent.BonusDuration;
                    }

                    break;
                }
            }
        }
    }
}

