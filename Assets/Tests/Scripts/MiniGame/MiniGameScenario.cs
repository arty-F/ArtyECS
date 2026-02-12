using ArtyECS.Core;
using UnityEngine;

public class MiniGameScenario : MonoBehaviour
{
    public PlayerSpawnConfig playerConfig;
    public EnemySpawnConfig enemyConfig;
    public CollectableSpawnConfig collectableConfig;

    private void Start()
    {
        World.RegisterSystem(new EnemySpawnSystem());
        World.RegisterSystem(new CollectableSpawnSystem());
        World.RegisterSystem(new InputSystem());
        World.RegisterSystem(new EnemyNavigateSystem());
        World.RegisterSystem(new MovementSystem());
        World.RegisterSystem(new CollectablePickupSystem());
        World.RegisterSystem(new ProximitySystem());
        World.RegisterSystem(new ExplosionSystem());
        World.RegisterSystem(new DestroyingSystem());
        World.RegisterSystem(new SpeedBonusExpireSystem());

        World.RegisterSystem(new TransformSyncSystem());
        World.RegisterSystem(new CameraFollowSystem());

        var configsCreatingSystem = new ConfigsCreatingSystem(playerConfig, enemyConfig, collectableConfig);
        configsCreatingSystem.Execute(World.Global);

        var playerRespawnSystem = new PlayerSpawnSystem();
        playerRespawnSystem.Execute(World.Global);
    }
}

