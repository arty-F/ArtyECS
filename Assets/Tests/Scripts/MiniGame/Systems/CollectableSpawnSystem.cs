using ArtyECS.Core;
using UnityEngine;

public class CollectableSpawnSystem : SystemHandler
{
    private float _timeUntilNextSpawn;

    public override void Execute(WorldInstance world)
    {
        _timeUntilNextSpawn -= Time.deltaTime;

        if (_timeUntilNextSpawn > 0f)
        {
            return;
        }

        var powerupSpawnConfig = world.GetUniqContext<CollectableSpawnConfig>();

        _timeUntilNextSpawn = powerupSpawnConfig.SpawnPeriod;

        var collectables = World.Query()
            .With<Collectable>()
            .Execute();

        var collectableCount = collectables.Count;
        if (collectableCount >= powerupSpawnConfig.MaxCollectables)
        {
            return;
        }
        
        var playerEntity = world.GetUniqContext<Player>().Entity;
        var playerPosition = playerEntity.Get<Position>();
        
        var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        var distance = Random.Range(powerupSpawnConfig.SpawnMinRange, powerupSpawnConfig.SpawnMaxRange);
        var spawnPosition = new Vector3(playerPosition.X + Mathf.Cos(angle) * distance, 0.25f, playerPosition.Z + Mathf.Sin(angle) * distance);

        var collectableGameObject = Object.Instantiate(powerupSpawnConfig.Prefab, spawnPosition, Quaternion.identity);
        var collectable = world.CreateEntity(collectableGameObject);
        var component = collectable.Add<Collectable>();
        component.SpeedBonus = powerupSpawnConfig.SpeedBonus;
        component.BonusDuration = powerupSpawnConfig.BonusDuration;
    }
}

