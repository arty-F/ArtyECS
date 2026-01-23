using ArtyECS.Core;
using System.Linq;
using UnityEngine;

public class EnemySpawnSystem : SystemHandler
{
    private readonly float _enemySpeed;
    private readonly GameObject _enemyPrefab;
    private readonly float _spawnPeriod;
    private readonly int _enemiesPerSpawn;
    private readonly int _maxEnemies;
    private readonly float _spawnMaxDistance;
    private readonly float _spawnMinDistance;
    private readonly float _explosionTriggerDistane;
    private float _timeUntilNextSpawn;

    public EnemySpawnSystem(float enemySpeed, GameObject enemyPrefab, float spawnPeriod, int enemiesPerSpawn, int maxEnemies, 
        float spawnMaxDistance, float spawnMinDistance, float explosionTriggerDistane)
    {
        _enemySpeed = enemySpeed;
        _enemyPrefab = enemyPrefab;
        _spawnPeriod = spawnPeriod;
        _enemiesPerSpawn = enemiesPerSpawn;
        _maxEnemies = maxEnemies;
        _timeUntilNextSpawn = spawnPeriod;
        _spawnMaxDistance = spawnMaxDistance;
        _spawnMinDistance = spawnMinDistance;
        _explosionTriggerDistane = explosionTriggerDistane;
    }

    public override void Execute(WorldInstance world)
    {
        _timeUntilNextSpawn -= Time.deltaTime;

        if (_timeUntilNextSpawn > 0f)
        {
            return;
        }

        _timeUntilNextSpawn = _spawnPeriod;

        var allEnemies = world
            .Query()
            .With<Enemy>()
            .Execute();
        var currentCount = allEnemies.Count();
        if (currentCount >= _maxEnemies)
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

        int enemiesToSpawn = Mathf.Min(_enemiesPerSpawn, _maxEnemies - currentCount);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var distance = Random.Range(_spawnMinDistance, _spawnMaxDistance);
            var spawnPosition = new Vector3( playerPosition.X + Mathf.Cos(angle) * distance, 0f, playerPosition.Z + Mathf.Sin(angle) * distance);
            var enemyGameObject = Object.Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
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
            speed.Value = _enemySpeed;
            var bomb = enemy.Add<ProximityBomb>();
            bomb.TriggerDistance = _explosionTriggerDistane;
        }
    }
}
