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

