using ArtyECS.Core;

public class ConfigsCreatingSystem : SystemHandler
{
    private PlayerSpawnConfig _playerConfig;
    private EnemySpawnConfig _enemyConfig;
    private CollectableSpawnConfig _collectableConfig;

    public ConfigsCreatingSystem(PlayerSpawnConfig playerConfig, EnemySpawnConfig enemyConfig, CollectableSpawnConfig collectableConfig)
    {
        _playerConfig = playerConfig;
        _enemyConfig = enemyConfig;
        _collectableConfig = collectableConfig;
    }

    public override void Execute(WorldInstance world)
    {
        var configsEntity = world.CreateEntity();
        configsEntity.AddUniq<PlayerSpawnConfig>(_playerConfig);
        configsEntity.AddUniq<EnemySpawnConfig>(_enemyConfig);
        configsEntity.AddUniq<CollectableSpawnConfig>(_collectableConfig);
    }
}

