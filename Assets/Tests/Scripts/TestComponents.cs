using ArtyECS.Core;


public class MyTestComponent : IComponent
{
    public int Value;
}

// Test components for tests
public class TestComponent : IComponent
{
    public int Value;
}

public class Position : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public class Velocity : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public class Health : IComponent
{
    public float Amount;
}

public class Dead : IComponent { }

public class Destroyed : IComponent { }

// Additional test components for System tests
public class CounterComponent : IComponent
{
    public int Value;
}

public class UpdateCounter : IComponent
{
    public int Value;
}

public class FixedUpdateCounter : IComponent
{
    public int Value;
}

public class Spawner : IComponent
{
    public int SpawnCount;
}

public class Acceleration : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

// Game components for performance scenarios
public class Enemy : IComponent { }

public class Armor : IComponent
{
    public float Value;
}

public class Damage : IComponent
{
    public float Value;
}

public class EnemyType : IComponent
{
    public int TypeId;
}

public class Powerup : IComponent { }

public class PowerupType : IComponent
{
    public int TypeId;
}

public class ExpirationTime : IComponent
{
    public float RemainingTime;
}

public class GameState : IComponent
{
    public int State;
}

public class PlayerStats : IComponent
{
    public int Level;
    public float Experience;
    public int Score;
}

public class WaveManager : IComponent
{
    public int CurrentWave;
    public float WaveTimer;
}

public class SpawnTimer : IComponent
{
    public float TimeUntilNextSpawn;
}

public class ScoreManager : IComponent
{
    public int TotalScore;
    public int HighScore;
}

public class TimeManager : IComponent
{
    public float GameTime;
    public float DeltaTime;
}

public class InputState : IComponent
{
    public float MoveX;
    public float MoveY;
}

public class CameraState : IComponent
{
    public float X;
    public float Y;
    public float Zoom;
}

public class UIControls : IComponent
{
    public bool PausePressed;
    public bool MenuPressed;
}

