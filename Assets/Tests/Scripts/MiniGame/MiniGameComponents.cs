using ArtyECS.Core;

public class Player : IComponent { }

public class MovementDirection : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public class Speed : IComponent
{
    public float Value;
}

public class SpeedBonus : IComponent
{
    public float BonusValue;
    public float RemainingTime;
}

public class EnemyExplosion : IComponent
{
    public float TimeRemaining;
    public float ExplosionRadius;
}

public class EnemyExplodingStarted : IComponent { }

public class EnemyExplodingAnimationStarted : IComponent { }

public class CollectableRadius : IComponent
{
    public float Radius;
}

public class CameraFollow : IComponent { }

public class EnemySpawnConfig : IComponent
{
    public float SpawnPeriod;
    public int EnemiesPerSpawn;
    public int MaxEnemies;
    public int CurrentCount;
    public float TimeUntilNextSpawn;
}

public class GameConfig : IComponent
{
    public float SpeedBonusValue;
    public float SpeedBonusDuration;
    public float EnemyProximityDistance;
    public float EnemyExplosionTime;
    public float EnemyExplosionRadius;
}

