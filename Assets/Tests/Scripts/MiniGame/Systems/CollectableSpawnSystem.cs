using ArtyECS.Core;
using UnityEngine;

public class CollectableSpawnSystem : SystemHandler
{
    private GameObject _prefab;
    private int _maxCollectables;
    private float _spawnMinRange;
    private float _spawnMaxRange;
    private float _speedBonus;
    private float _bonusDuration;
    private float _spawnDuration;
    private float _timeUntilNextSpawn;

    public CollectableSpawnSystem(GameObject prefab, int maxCollectables, float spawnDuration, float spawnMinRange, float spawnMaxRange, float speedBonus, float bonusDuration)
    {
        _prefab = prefab;
        _maxCollectables = maxCollectables;
        _spawnMinRange = spawnMinRange;
        _spawnMaxRange = spawnMaxRange;
        _speedBonus = speedBonus;
        _bonusDuration = bonusDuration;
        _spawnDuration = spawnDuration;
        _timeUntilNextSpawn = spawnDuration;
    }

    public override void Execute(WorldInstance world)
    {
        _timeUntilNextSpawn -= Time.deltaTime;

        if (_timeUntilNextSpawn > 0f)
        {
            return;
        }
        _timeUntilNextSpawn = _spawnDuration;

        var collectables = World.Query()
            .With<Collectable>()
            .Execute();

        var collectableCount = collectables.Count;
        if (collectableCount >= _maxCollectables)
        {
            return;
        }
        
        Entity playerEntity = null;
        Position playerPosition = default;
        var playerEntities = world
            .Query()
            .With<Player>()
            .Execute();
        foreach (var player in playerEntities)
        {
            playerEntity = player;
            playerPosition = player.Get<Position>();
            break;
        }
        
        var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        var distance = Random.Range(_spawnMinRange, _spawnMaxRange);
        var spawnPosition = new Vector3(playerPosition.X + Mathf.Cos(angle) * distance, 0.25f, playerPosition.Z + Mathf.Sin(angle) * distance);

        var collectableGameObject = Object.Instantiate(_prefab, spawnPosition, Quaternion.identity);
        var collectable = world.CreateEntity(collectableGameObject);
        var component = collectable.Add<Collectable>();
        component.SpeedBonus = _speedBonus;
        component.BonusDuration = _bonusDuration;
    }
}

