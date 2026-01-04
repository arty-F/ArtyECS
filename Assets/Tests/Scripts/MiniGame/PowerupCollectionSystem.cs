using ArtyECS.Core;
using UnityEngine;

public class PowerupCollectionSystem : SystemHandler
{
    private float _speedBonusValue;
    private float _speedBonusDuration;

    public PowerupCollectionSystem(float speedBonusValue, float speedBonusDuration)
    {
        _speedBonusValue = speedBonusValue;
        _speedBonusDuration = speedBonusDuration;
    }

    public override void Execute(WorldInstance world)
    {
        Position playerPosition = default;
        CollectableRadius collectableRadius = default;

        var playerEntities = world.Query().With<Player>().With<Position>().With<CollectableRadius>().Execute();
        foreach (var player in playerEntities)
        {
            playerPosition = player.GetComponent<Position>();
            collectableRadius = player.GetComponent<CollectableRadius>();
            break;
        }

        var powerups = world.Query().With<Powerup>().With<Position>().Execute();
        foreach (var powerup in powerups)
        {
            var powerupPosition = powerup.GetComponent<Position>();

            float distance = Vector3.Distance(
                new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z),
                new Vector3(powerupPosition.X, powerupPosition.Y, powerupPosition.Z)
            );

            if (distance <= collectableRadius.Radius)
            {
                var playersWithBonus = world.Query().With<Player>().With<SpeedBonus>().Execute();
                foreach (var player in playersWithBonus)
                {
                    var speedBonus = player.GetComponent<SpeedBonus>();
                    speedBonus.BonusValue += _speedBonusValue;
                    speedBonus.RemainingTime = _speedBonusDuration;
                    player.RemoveComponent(speedBonus);
                    player.AddComponent(speedBonus);
                    world.DestroyEntity(powerup);
                    return;
                }

                var playersWithoutBonus = world.Query().With<Player>().Without<SpeedBonus>().Execute();
                foreach (var player in playersWithoutBonus)
                {
                    var speedBonus = new SpeedBonus
                    {
                        BonusValue = _speedBonusValue,
                        RemainingTime = _speedBonusDuration
                    };
                    player.AddComponent(speedBonus);
                    world.DestroyEntity(powerup);
                    return;
                }
            }
        }
    }
}
