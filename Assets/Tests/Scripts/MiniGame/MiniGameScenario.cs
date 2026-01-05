using ArtyECS.Core;
using UnityEngine;

public class MiniGameScenario : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float playerBaseSpeed = 4f;
    [SerializeField] private float enemySpeed = 0.5f;
    [SerializeField] private float enemySpawnPeriod = 0.5f;
    [SerializeField] private int enemiesPerSpawn = 500;
    [SerializeField] private int maxEnemies = 5000;
    [SerializeField] private float _enemySpawnMinDistance = 20f;
    [SerializeField] private float _enemySpawnMaxDistance = 40f;
    [SerializeField] private float enemyExplosionTime = 1f;
    [SerializeField] private float enemyExplosionRadius = 2f;
    [SerializeField] private float enemyProximityDistance = 1.5f;
    [SerializeField] private float powerupCollectionRadius = 1f;
    [SerializeField] private int _maxPowerups = 100;
    [SerializeField] private float _powerupRespawnInterval = 0.2f;
    [SerializeField] private float speedBonusValue = 1f;
    [SerializeField] private float speedBonusDuration = 5f;

    private void Start()
    {
        var playerGameObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        var player = World.CreateEntity(playerGameObject);
        player.AddComponent(new Player());
        player.AddComponent(new Position());
        player.AddComponent(new MoveDirection());
        player.AddComponent(new Speed { Value = playerBaseSpeed });
        player.AddComponent(new Health { Amount = 100 });
        player.AddComponent(new CollectablePickuper() { PickupRange = powerupCollectionRadius });

        //entity.AddTag<>
        //world.GetTagged<>

        World.RegisterSystem(new EnemySpawnSystem(enemySpeed, enemyPrefab, enemySpawnPeriod, enemiesPerSpawn, maxEnemies, 
            _enemySpawnMinDistance, _enemySpawnMaxDistance, enemyProximityDistance));
        World.RegisterSystem(new CollectableSpawnSystem(powerupPrefab, _maxPowerups, _powerupRespawnInterval, _enemySpawnMinDistance,
            _enemySpawnMaxDistance, speedBonusValue, speedBonusDuration));
        World.RegisterSystem(new InputSystem());
        World.RegisterSystem(new EnemyNavigateSystem());
        World.RegisterSystem(new MovementSystem());
        World.RegisterSystem(new CollectablePickupSystem());
        World.RegisterSystem(new ProximitySystem(enemyExplosionTime, enemyExplosionRadius));
        World.RegisterSystem(new ExplosionSystem());
        World.RegisterSystem(new DestroyingSystem());
        World.RegisterSystem(new SpeedBonusExpireSystem());

        World.RegisterSystem(new TransformSyncSystem());
        World.RegisterSystem(new CameraFollowSystem());
    }
}

