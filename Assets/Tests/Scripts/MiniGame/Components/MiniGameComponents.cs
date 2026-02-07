using ArtyECS.Core;

public class Player : Component { }

public class Enemy : Component { }

public class Config : Component { }

public class Position : Component 
{
    public float X;
    public float Y;
    public float Z;
}

public class Health : Component
{
    public float Amount;
}

public class MoveDirection : Component
{
    public float X;
    public float Y;
    public float Z;
}

public class Speed : Component
{
    public float Value;
}

public class ProximityBomb : Component 
{
    public float TriggerDistance;
}

public class Explosion : Component
{
    public float TimeRemaining;
    public float ExplosionRadius;
}

public class Destroying : Component { } 

public class Collectable : Component 
{
    public float SpeedBonus;
    public float BonusDuration;
}

public class CollectablePickuper : Component 
{
    public float PickupRange;
}

public class SpeedBonus : Component
{
    public float Value;
    public float TimeRamaining;
}