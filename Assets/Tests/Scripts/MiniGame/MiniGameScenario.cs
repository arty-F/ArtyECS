using ArtyECS.Core;
using UnityEngine;

public class MiniGameScenario : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float playerBaseSpeed = 5f;
    [SerializeField] private float enemySpeed = 2f;
    [SerializeField] private float enemySpawnPeriod = 3f;
    [SerializeField] private int enemiesPerSpawn = 3;
    [SerializeField] private int maxEnemies = 20;
    [SerializeField] private float enemyExplosionTime = 2f;
    [SerializeField] private float enemyExplosionRadius = 3f;
    [SerializeField] private float enemyProximityDistance = 2f;
    [SerializeField] private float powerupCollectionRadius = 1.5f;
    [SerializeField] private float speedBonusValue = 2f;
    [SerializeField] private float speedBonusDuration = 5f;

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject PowerupPrefab => powerupPrefab;
    public GameObject EnemyPrefab => enemyPrefab;
    public float EnemySpeed => enemySpeed;

    private WorldInstance world;

    private void Start()
    {
        world = World.Global;

        CreatePlayerEntity();
        CreateCameraEntity();

        RegisterSystems();
    }

    private void OnDestroy()
    {
        World.Clear();
    }

    private void CreatePlayerEntity()
    {
        GameObject playerGameObject = null;
        if (playerPrefab != null)
        {
            playerGameObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            playerGameObject = new GameObject("Player");
            playerGameObject.transform.position = Vector3.zero;
        }

        var player = world.CreateEntity(playerGameObject);
        player.AddComponent(new Player());
        player.AddComponent(new Position { X = 0f, Y = 0f, Z = 0f });
        player.AddComponent(new MovementDirection { X = 0f, Y = 0f, Z = 0f });
        player.AddComponent(new Speed { Value = playerBaseSpeed });
        player.AddComponent(new Health { Amount = 100f });
        player.AddComponent(new CollectableRadius { Radius = powerupCollectionRadius });
    }

    private void CreateCameraEntity()
    {
        var cameraEntity = world.CreateEntity();
        cameraEntity.AddComponent(new CameraFollow());
    }


    public Entity CreateEnemyEntity(Vector3 position)
    {
        GameObject enemyGameObject = null;
        if (enemyPrefab != null)
        {
            enemyGameObject = Instantiate(enemyPrefab, position, Quaternion.identity);
        }
        else
        {
            enemyGameObject = new GameObject("Enemy");
            enemyGameObject.transform.position = position;
        }

        var enemy = world.CreateEntity(enemyGameObject);
        enemy.AddComponent(new Enemy());
        enemy.AddComponent(new Position { X = position.x, Y = position.y, Z = position.z });
        enemy.AddComponent(new MovementDirection { X = 0f, Y = 0f, Z = 0f });
        enemy.AddComponent(new Speed { Value = enemySpeed });

        return enemy;
    }

    public Entity CreatePowerupEntity(Vector3 position)
    {
        GameObject powerupGameObject = null;
        if (powerupPrefab != null)
        {
            powerupGameObject = Instantiate(powerupPrefab, position, Quaternion.identity);
        }
        else
        {
            powerupGameObject = new GameObject("Powerup");
            powerupGameObject.transform.position = position;
        }

        var powerup = world.CreateEntity(powerupGameObject);
        powerup.AddComponent(new Powerup());
        powerup.AddComponent(new Position { X = position.x, Y = position.y, Z = position.z });

        return powerup;
    }

    private void RegisterSystems()
    {
        world.RegisterSystem(new InputSystem());
        world.RegisterSystem(new EnemyDirectionSystem());
        world.RegisterSystem(new MovementSystem());


        world.RegisterSystem(new EnemySpawnSystem(enemySpeed, enemyExplosionTime, enemyExplosionRadius, enemyPrefab, enemySpawnPeriod, enemiesPerSpawn, maxEnemies));

        world.RegisterSystem(new EnemyProximitySystem(enemyProximityDistance,enemyExplosionTime, enemyExplosionRadius));
        world.RegisterSystem(new EnemyAnimationSystem());



        //world.RegisterSystem(new SpeedBonusSystem(), UpdateType.Update);
        world.RegisterSystem(new EnemyExplosionSystem(), UpdateType.Update);
        //world.RegisterSystem(new PowerupCollectionSystem(speedBonusValue, speedBonusDuration), UpdateType.Update);
        world.RegisterSystem(new TransformSyncSystem(), UpdateType.Update);
        world.RegisterSystem(new CameraFollowSystem(), UpdateType.Update);
    }
}

