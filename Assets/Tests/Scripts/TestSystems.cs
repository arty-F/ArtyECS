using ArtyECS.Core;
using System;
using UnityEngine;

// Test systems for tests
public class TestSystem : SystemHandler
{
    private Action executeAction;

    public TestSystem(Action executeAction)
    {
        this.executeAction = executeAction;
    }

    public override void Execute(WorldInstance world)
    {
        executeAction?.Invoke();
    }
}

public class StatefulSystem : SystemHandler
{
    public int FieldValue;
}

public class IncrementCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<CounterComponent>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.Get<CounterComponent>();
            counter.Value++;
        }
    }
}

public class IncrementUpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<UpdateCounter>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.Get<UpdateCounter>();
            counter.Value++;
        }
    }
}

public class IncrementFixedUpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<FixedUpdateCounter>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.Get<FixedUpdateCounter>();
            counter.Value++;
        }
    }
}

public class MovementTestSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Position>().With<Velocity>().Execute();
        foreach (var entity in entities)
        {
            var position = entity.Get<Position>();
            var velocity = entity.Get<Velocity>();

            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;
        }
    }
}

public class DamageSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Health>().With<Damage>().Execute();
        foreach (var entity in entities)
        {
            var health = entity.Get<Health>();
            var damage = entity.Get<Damage>();

            health.Amount -= damage.Value;

            //entity.RemoveComponent(typeof(Damage));
        }
    }
}

public class ExecutionOrderTrackerSystem : SystemHandler
{
    public int ExecutionOrder { get; set; }
    public static int GlobalExecutionOrder = 0;

    public override void Execute(WorldInstance world)
    {
        ExecutionOrder = GlobalExecutionOrder++;
    }
}