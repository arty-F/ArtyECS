using ArtyECS.Core;
using UnityEngine;

public class EnemySpawnSystem : SystemHandler
{
    private float _enemySpeed;
    private float _enemyExplosionTime;
    private float _enemyExplosionRadius;
    private GameObject _enemyPrefab;
    private float _spawnPeriod;
    private int _enemiesPerSpawn;
    private int _maxEnemies;
    private float _timeUntilNextSpawn;

    public EnemySpawnSystem(float enemySpeed, float enemyExplosionTime, float enemyExplosionRadius, GameObject enemyPrefab, float spawnPeriod, int enemiesPerSpawn, int maxEnemies)
    {
        _enemySpeed = enemySpeed;
        _enemyExplosionTime = enemyExplosionTime;
        _enemyExplosionRadius = enemyExplosionRadius;
        _enemyPrefab = enemyPrefab;
        _spawnPeriod = spawnPeriod;
        _enemiesPerSpawn = enemiesPerSpawn;
        _maxEnemies = maxEnemies;
        _timeUntilNextSpawn = spawnPeriod;
    }

    public override void Execute(WorldInstance world)
    {
        _timeUntilNextSpawn -= Time.deltaTime;

        if (_timeUntilNextSpawn <= 0f)
        {
            var allEnemies = world.Query().With<Enemy>().Execute();
            int currentCount = 0;
            foreach (var enemy in allEnemies)
            {
                currentCount++;
            }

            if (currentCount < _maxEnemies)
            {
                Entity playerEntity = null;
                Position playerPosition = default;

                var playerEntities = world.Query().With<Player>().With<Position>().Execute();
                foreach (var player in playerEntities)
                {
                    playerEntity = player;
                    playerPosition = player.GetComponent<Position>();
                    break;
                }

                if (playerEntity != null)
                {
                    int enemiesToSpawn = Mathf.Min(
                        _enemiesPerSpawn,
                        _maxEnemies - currentCount
                    );

                    for (int i = 0; i < enemiesToSpawn; i++)
                    {
                        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                        float distance = Random.Range(10f, 20f);
                        Vector3 spawnPosition = new Vector3(
                            playerPosition.X + Mathf.Cos(angle) * distance,
                            0f,
                            playerPosition.Z + Mathf.Sin(angle) * distance
                        );

                        GameObject enemyGameObject = null;
                        if (_enemyPrefab != null)
                        {
                            enemyGameObject = Object.Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
                        }
                        else
                        {
                            enemyGameObject = new GameObject("Enemy");
                            enemyGameObject.transform.position = spawnPosition;
                        }

                        var enemy = world.CreateEntity(enemyGameObject);
                        enemy.AddComponent(new Enemy());
                        enemy.AddComponent(new Position { X = spawnPosition.x, Y = spawnPosition.y, Z = spawnPosition.z });
                        enemy.AddComponent(new MovementDirection { X = 0f, Y = 0f, Z = 0f });
                        enemy.AddComponent(new Speed { Value = _enemySpeed });
                    }
                }
            }

            _timeUntilNextSpawn = _spawnPeriod;
        }
    }
}
