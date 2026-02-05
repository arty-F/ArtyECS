using ArtyECS.Core;
using UnityEngine;

public class PlayerSpawnSystem : SystemHandler
{
    private GameObject _playerPrefab;
    private float _playerBaseSpeed;
    private float _powerupCollectionRadius;

    public PlayerSpawnSystem(GameObject playerPrefab, float playerBaseSpeed, float powerupCollectionRadius)
    {
        _playerPrefab = playerPrefab;
        _playerBaseSpeed = playerBaseSpeed;
        _powerupCollectionRadius = powerupCollectionRadius;
    }

    public override void Execute(WorldInstance world)
    {
        var playerGameObject = UnityEngine.Object.Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
        var player = World.CreateEntity(playerGameObject);
        player.Add<Player>();
        player.Add<Position>();
        player.Add<MoveDirection>();
        var speed = player.Add<Speed>();
        speed.Value = _playerBaseSpeed;
        var health = player.Add<Health>();
        health.Amount = 100;
        var pickuper = player.Add<CollectablePickuper>();
        pickuper.PickupRange = _powerupCollectionRadius;
    }
}

