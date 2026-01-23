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
        player.AddComponent(new Player());
        player.AddComponent(new Position());
        player.AddComponent(new MoveDirection());
        player.AddComponent(new Speed { Value = _playerBaseSpeed });
        player.AddComponent(new Health { Amount = 100 });
        player.AddComponent(new CollectablePickuper() { PickupRange = _powerupCollectionRadius });
    }
}

