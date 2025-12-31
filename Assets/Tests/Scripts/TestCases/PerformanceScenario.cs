using UnityEngine;
using ArtyECS.Core;

public enum PerformanceTestType
{
    GetWith1,
    GetWith2,
    GetWith3,
    GetWithout1,
    GetWithout2,
    GetWithout3,
    Modifiable,
    QueryBuilderMixed,
    ComparativeWith1,
    ComparativeWith2,
    ComparativeWithout1,
    ComparativeWithout2,
    ComparativeWithout3,
    AllSystems
}

public enum ComponentDistributionPattern
{
    Sparse,
    Dense,
    Balanced
}

public class PerformanceScenario : MonoBehaviour
{
    [SerializeField] private int enemyCount = 5000;
    [SerializeField] private int powerupCount = 300;
    [SerializeField] private ComponentDistributionPattern distributionPattern = ComponentDistributionPattern.Balanced;
    [SerializeField] private PerformanceTestType testType = PerformanceTestType.AllSystems;
    
    private void Start()
    {
        var world = World.GetOrCreate();
        SetupWorld(world, enemyCount, powerupCount, distributionPattern);
        
        switch (testType)
        {
            case PerformanceTestType.GetWith1:
                world.AddToUpdate(new EnemyQueryWith1System());
                break;
            case PerformanceTestType.GetWith2:
                world.AddToUpdate(new EnemyMovementWith2System());
                break;
            case PerformanceTestType.GetWith3:
                world.AddToUpdate(new EnemyCombatWith3System());
                break;
            case PerformanceTestType.GetWithout1:
                world.AddToUpdate(new AliveEnemyWithout1System());
                break;
            case PerformanceTestType.GetWithout2:
                world.AddToUpdate(new ActiveEntitiesWithout2System());
                break;
            case PerformanceTestType.GetWithout3:
                world.AddToUpdate(new CleanEntitiesWithout3System());
                break;
            case PerformanceTestType.Modifiable:
                world.AddToUpdate(new EnemyHealthModifiableSystem());
                break;
            case PerformanceTestType.QueryBuilderMixed:
                world.AddToUpdate(new ActiveEnemyMovementQueryBuilderMixedSystem());
                break;
            case PerformanceTestType.ComparativeWith1:
                world.AddToUpdate(new EnemyQueryWith1DirectSystem());
                world.AddToUpdate(new EnemyQueryWith1QueryBuilderSystem());
                break;
            case PerformanceTestType.ComparativeWith2:
                world.AddToUpdate(new EnemyMovementWith2DirectSystem());
                world.AddToUpdate(new EnemyMovementWith2QueryBuilderSystem());
                break;
            case PerformanceTestType.ComparativeWithout1:
                world.AddToUpdate(new AliveEnemyWithout1DirectSystem());
                world.AddToUpdate(new AliveEnemyWithout1QueryBuilderSystem());
                break;
            case PerformanceTestType.ComparativeWithout2:
                world.AddToUpdate(new ActiveEntitiesWithout2DirectSystem());
                world.AddToUpdate(new ActiveEntitiesWithout2QueryBuilderSystem());
                break;
            case PerformanceTestType.ComparativeWithout3:
                world.AddToUpdate(new CleanEntitiesWithout3DirectSystem());
                world.AddToUpdate(new CleanEntitiesWithout3QueryBuilderSystem());
                break;
            case PerformanceTestType.AllSystems:
                world.AddToUpdate(new EnemyQueryWith1System());
                world.AddToUpdate(new EnemyMovementWith2System());
                world.AddToUpdate(new EnemyCombatWith3System());
                world.AddToUpdate(new AliveEnemyWithout1System());
                world.AddToUpdate(new ActiveEntitiesWithout2System());
                world.AddToUpdate(new CleanEntitiesWithout3System());
                world.AddToUpdate(new EnemyHealthModifiableSystem());
                world.AddToUpdate(new ActiveEnemyMovementQueryBuilderMixedSystem());
                break;
        }
    }
    
    private void SetupWorld(WorldInstance world, int enemyCount, int powerupCount, ComponentDistributionPattern pattern)
    {
        switch (pattern)
        {
            case ComponentDistributionPattern.Sparse:
                CreateSparseDistribution(world, enemyCount, powerupCount);
                break;
            case ComponentDistributionPattern.Dense:
                CreateDenseDistribution(world, enemyCount, powerupCount);
                break;
            case ComponentDistributionPattern.Balanced:
            default:
                CreateBalancedDistribution(world, enemyCount, powerupCount);
                break;
        }
    }
    
    private void CreateSparseDistribution(WorldInstance world, int enemyCount, int powerupCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Enemy());
            
            int group = i % 3;
            if (group == 0)
            {
                world.AddComponent(entity, new Position 
                { 
                    X = Random.Range(-100f, 100f), 
                    Y = Random.Range(-100f, 100f), 
                    Z = Random.Range(-100f, 100f) 
                });
                world.AddComponent(entity, new Health 
                { 
                    Amount = Random.Range(50f, 200f) 
                });
            }
            else if (group == 1)
            {
                world.AddComponent(entity, new Velocity 
                { 
                    X = Random.Range(-1f, 1f), 
                    Y = Random.Range(-1f, 1f), 
                    Z = Random.Range(-1f, 1f) 
                });
                world.AddComponent(entity, new Damage 
                { 
                    Value = Random.Range(1f, 50f) 
                });
            }
            else
            {
                world.AddComponent(entity, new Armor 
                { 
                    Value = Random.Range(0f, 100f) 
                });
            }
        }
        
        for (int i = 0; i < powerupCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Powerup());
            world.AddComponent(entity, new PowerupType 
            { 
                TypeId = Random.Range(0, 5) 
            });
            world.AddComponent(entity, new Position 
            { 
                X = Random.Range(-100f, 100f), 
                Y = Random.Range(-100f, 100f), 
                Z = Random.Range(-100f, 100f) 
            });
        }
        
        CreateGameStateEntities(world);
    }
    
    private void CreateDenseDistribution(WorldInstance world, int enemyCount, int powerupCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Enemy());
            
            if (Random.value > 0.1f)
            {
                world.AddComponent(entity, new Position 
                { 
                    X = Random.Range(-100f, 100f), 
                    Y = Random.Range(-100f, 100f), 
                    Z = Random.Range(-100f, 100f) 
                });
                world.AddComponent(entity, new Velocity 
                { 
                    X = Random.Range(-1f, 1f), 
                    Y = Random.Range(-1f, 1f), 
                    Z = Random.Range(-1f, 1f) 
                });
                world.AddComponent(entity, new Health 
                { 
                    Amount = Random.Range(50f, 200f) 
                });
                world.AddComponent(entity, new Damage 
                { 
                    Value = Random.Range(1f, 50f) 
                });
                world.AddComponent(entity, new Armor 
                { 
                    Value = Random.Range(0f, 100f) 
                });
            }
            
            if (Random.value > 0.9f)
            {
                world.AddComponent(entity, new Dead());
            }
        }
        
        for (int i = 0; i < powerupCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Powerup());
            world.AddComponent(entity, new PowerupType 
            { 
                TypeId = Random.Range(0, 5) 
            });
            world.AddComponent(entity, new Position 
            { 
                X = Random.Range(-100f, 100f), 
                Y = Random.Range(-100f, 100f), 
                Z = Random.Range(-100f, 100f) 
            });
            world.AddComponent(entity, new ExpirationTime 
            { 
                RemainingTime = Random.Range(1f, 10f) 
            });
        }
        
        CreateGameStateEntities(world);
    }
    
    private void CreateBalancedDistribution(WorldInstance world, int enemyCount, int powerupCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Enemy());
            
            if (Random.value > 0.5f)
            {
                world.AddComponent(entity, new Position 
                { 
                    X = Random.Range(-100f, 100f), 
                    Y = Random.Range(-100f, 100f), 
                    Z = Random.Range(-100f, 100f) 
                });
            }
            
            if (Random.value > 0.6f)
            {
                world.AddComponent(entity, new Velocity 
                { 
                    X = Random.Range(-1f, 1f), 
                    Y = Random.Range(-1f, 1f), 
                    Z = Random.Range(-1f, 1f) 
                });
            }
            
            if (Random.value > 0.7f)
            {
                world.AddComponent(entity, new Health 
                { 
                    Amount = Random.Range(50f, 200f) 
                });
            }
            
            if (Random.value > 0.2f)
            {
                world.AddComponent(entity, new Armor 
                { 
                    Value = Random.Range(0f, 100f) 
                });
            }
            
            if (Random.value > 0.1f)
            {
                world.AddComponent(entity, new Damage 
                { 
                    Value = Random.Range(1f, 50f) 
                });
            }
            
            if (Random.value > 0.3f)
            {
                world.AddComponent(entity, new EnemyType 
                { 
                    TypeId = Random.Range(0, 9) 
                });
            }
            
            if (Random.value > 0.9f)
            {
                world.AddComponent(entity, new Dead());
            }
        }
        
        for (int i = 0; i < powerupCount; i++)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new Powerup());
            world.AddComponent(entity, new PowerupType 
            { 
                TypeId = Random.Range(0, 5) 
            });
            world.AddComponent(entity, new Position 
            { 
                X = Random.Range(-100f, 100f), 
                Y = Random.Range(-100f, 100f), 
                Z = Random.Range(-100f, 100f) 
            });
            
            if (Random.value > 0.2f)
            {
                world.AddComponent(entity, new ExpirationTime 
                { 
                    RemainingTime = Random.Range(1f, 10f) 
                });
            }
        }
        
        CreateGameStateEntities(world);
    }
    
    private void CreateGameStateEntities(WorldInstance world)
    {
        var gameStateEntity = world.CreateEntity();
        world.AddComponent(gameStateEntity, new GameState { State = 1 });
        
        var playerStatsEntity = world.CreateEntity();
        world.AddComponent(playerStatsEntity, new PlayerStats { Level = 1, Experience = 0f, Score = 0 });
        
        var waveManagerEntity = world.CreateEntity();
        world.AddComponent(waveManagerEntity, new WaveManager { CurrentWave = 1, WaveTimer = 0f });
        
        var spawnTimerEntity = world.CreateEntity();
        world.AddComponent(spawnTimerEntity, new SpawnTimer { TimeUntilNextSpawn = 0f });
        
        var scoreManagerEntity = world.CreateEntity();
        world.AddComponent(scoreManagerEntity, new ScoreManager { TotalScore = 0, HighScore = 0 });
        
        var timeManagerEntity = world.CreateEntity();
        world.AddComponent(timeManagerEntity, new TimeManager { GameTime = 0f, DeltaTime = 0f });
        
        var inputStateEntity = world.CreateEntity();
        world.AddComponent(inputStateEntity, new InputState { MoveX = 0f, MoveY = 0f });
        
        var cameraStateEntity = world.CreateEntity();
        world.AddComponent(cameraStateEntity, new CameraState { X = 0f, Y = 0f, Zoom = 1f });
        
        var uiControlsEntity = world.CreateEntity();
        world.AddComponent(uiControlsEntity, new UIControls { PausePressed = false, MenuPressed = false });
    }
}
