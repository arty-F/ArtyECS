using ArtyECS.Core;

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

