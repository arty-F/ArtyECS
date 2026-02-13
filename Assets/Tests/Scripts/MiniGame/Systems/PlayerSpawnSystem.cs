using ArtyECS.Core;
using UnityEngine;

public class PlayerSpawnSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var playerSpawnConfig = world
            .GetUniqContext<PlayerSpawnConfig>();

        var playerGameObject = UnityEngine.Object.Instantiate(playerSpawnConfig.Prefab, Vector3.zero, Quaternion.identity);
        var player = World.CreateEntity(playerGameObject);
        player.AddUniq<Player>(null);
        player.Add<Position>();
        player.Add<MoveDirection>();
        var speed = player.Add<Speed>();
        speed.Value = playerSpawnConfig.PlayerBaseSpeed;
        var health = player.Add<Health>();
        health.Amount = playerSpawnConfig.Health;
        var pickuper = player.Add<CollectablePickuper>();
        pickuper.PickupRange = playerSpawnConfig.PowerupCollectionRadius;
    }
}

