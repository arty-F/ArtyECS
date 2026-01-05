using ArtyECS.Core;
using System.Linq;
using UnityEngine;

public class CollectablePickupSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var collectablePickupers = World.Query()
            .With<CollectablePickuper>()
            .Execute()
            .ToArray();

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

                var pickupComponent = pickuper.GetComponent<CollectablePickuper>();

                if (Vector3.Distance(collectable.GameObject.transform.position, pickuper.GameObject.transform.position) < pickupComponent.PickupRange)
                {
                    collectable.AddComponent(new Destroying());
                    var collectableComponent = collectable.GetComponent<Collectable>();
                    if (!pickuper.HasComponent<SpeedBonus>())
                    {
                        pickuper.AddComponent(new SpeedBonus { Value = collectableComponent.SpeedBonus, TimeRamaining = collectableComponent.BonusDuration });
                    }
                    else
                    {
                        var speedBonus = pickuper.GetComponent<SpeedBonus>();
                        speedBonus.Value += collectableComponent.SpeedBonus;
                        speedBonus.TimeRamaining = collectableComponent.BonusDuration;
                    }

                    break;
                }
            }
        }
    }
}

