using ArtyECS.Core;


public class MyTestComponent : IComponent
{
    public int Value;
}

// Test components for tests
public struct TestComponent : IComponent
{
    public int Value;
}

public struct Position : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public struct Velocity : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public struct Health : IComponent
{
    public float Amount;
}

public struct Dead : IComponent { }

public struct Destroyed : IComponent { }

// Additional test components for System tests
public struct CounterComponent : IComponent
{
    public int Value;
}

public struct UpdateCounter : IComponent
{
    public int Value;
}

public struct FixedUpdateCounter : IComponent
{
    public int Value;
}

public struct Spawner : IComponent
{
    public int SpawnCount;
}

public struct Acceleration : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

// Game components for performance scenarios
public struct Enemy : IComponent { }

public struct Armor : IComponent
{
    public float Value;
}

public struct Damage : IComponent
{
    public float Value;
}

public struct EnemyType : IComponent
{
    public int TypeId;
}

public struct Powerup : IComponent { }

public struct PowerupType : IComponent
{
    public int TypeId;
}

public struct ExpirationTime : IComponent
{
    public float RemainingTime;
}

public struct GameState : IComponent
{
    public int State;
}

public struct PlayerStats : IComponent
{
    public int Level;
    public float Experience;
    public int Score;
}

public struct WaveManager : IComponent
{
    public int CurrentWave;
    public float WaveTimer;
}

public struct SpawnTimer : IComponent
{
    public float TimeUntilNextSpawn;
}

public struct ScoreManager : IComponent
{
    public int TotalScore;
    public int HighScore;
}

public struct TimeManager : IComponent
{
    public float GameTime;
    public float DeltaTime;
}

public struct InputState : IComponent
{
    public float MoveX;
    public float MoveY;
}

public struct CameraState : IComponent
{
    public float X;
    public float Y;
    public float Zoom;
}

public struct UIControls : IComponent
{
    public bool PausePressed;
    public bool MenuPressed;
}

