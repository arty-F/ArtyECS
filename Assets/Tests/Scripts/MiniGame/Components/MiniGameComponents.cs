using ArtyECS.Core;

public class Player : IComponent { }

public class MoveDirection : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public class Speed : IComponent
{
    public float Value;
}

public class ProximityBomb : IComponent 
{
    public float TriggerDistance;
}

public class Explosion : IComponent
{
    public float TimeRemaining;
    public float ExplosionRadius;
}

public class Destroying : IComponent { } 

public class Collectable : IComponent 
{
    public float SpeedBonus;
    public float BonusDuration;
}

public class CollectablePickuper : IComponent 
{
    public float PickupRange;
}

public class SpeedBonus : IComponent
{
    public float Value;
    public float TimeRamaining;
}