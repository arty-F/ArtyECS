using ArtyECS.Core;
using System.Linq;
using UnityEngine;

public class EnemySpawnSystem : SystemHandler
{
    private float _timeUntilNextSpawn;

    public override void Execute(WorldInstance world)
    {
        _timeUntilNextSpawn -= Time.deltaTime;

        if (_timeUntilNextSpawn > 0f)
        {
            return;
        }

        var enemySpawnConfig = world
            .GetUniqEntity<Config>()
            .Get<EnemySpawnConfig>();

        _timeUntilNextSpawn = enemySpawnConfig.SpawnPeriod;

        var allEnemies = world
            .Query()
            .With<Enemy>()
            .Execute();
        var currentCount = allEnemies.Count();
        if (currentCount >= enemySpawnConfig.MaxEnemies)
        {
            return;
        }

        var playerEntity = world.GetUniqEntity<Player>();
        var playerPosition = playerEntity.Get<Position>();

        int enemiesToSpawn = Mathf.Min(enemySpawnConfig.EnemiesPerSpawn, enemySpawnConfig.MaxEnemies - currentCount);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var distance = Random.Range(enemySpawnConfig.SpawnMinDistance, enemySpawnConfig.SpawnMaxDistance);
            var spawnPosition = new Vector3( playerPosition.X + Mathf.Cos(angle) * distance, 0f, playerPosition.Z + Mathf.Sin(angle) * distance);
            var enemyGameObject = Object.Instantiate(enemySpawnConfig.Prefab, spawnPosition, Quaternion.identity);
            var enemy = world.CreateEntity(enemyGameObject);
            enemy.Add<Enemy>();
            var position = enemy.Add<Position>();
            position.X = spawnPosition.x;
            position.Y = spawnPosition.y;
            position.Z = spawnPosition.z;
            var moveDirection = enemy.Add<MoveDirection>();
            moveDirection.X = 0f;
            moveDirection.Y = 0f;
            moveDirection.Z = 0f;
            var speed = enemy.Add<Speed>();
            speed.Value = enemySpawnConfig.EnemySpeed;
            var bomb = enemy.Add<ProximityBomb>();
            bomb.TriggerDistance = enemySpawnConfig.ExplosionTriggerDistane;
        }
    }
}
