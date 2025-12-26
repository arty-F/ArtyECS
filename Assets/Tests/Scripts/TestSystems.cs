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

public class IncrementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var counters = world.GetModifiableComponents<CounterComponent>();
        for (int i = 0; i < counters.Count; i++)
        {
            counters[i].Value++;
        }
    }
}

public class MovementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.GetEntitiesWith<Position, Velocity>();
        
        foreach (var entity in entities)
        {
            Velocity velocity = world.GetComponent<Velocity>(entity);
            
            ref var position = ref world.GetModifiableComponent<Position>(entity);
            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;
        }
    }
}

public class HealthSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var healths = world.GetModifiableComponents<Health>();
        for (int i = 0; i < healths.Count; i++)
        {
            healths[i].Amount -= 1f;
        }
    }
}

public class ModifiableHealthSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var healths = world.GetModifiableComponents<Health>();
        for (int i = 0; i < healths.Count; i++)
        {
            healths[i].Amount -= 1f;
        }
    }
}

public class PhysicsSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Update velocities with acceleration
        var entitiesWithAccel = world.GetEntitiesWith<Velocity, Acceleration>();
        foreach (var entity in entitiesWithAccel)
        {
            Velocity velocity = world.GetComponent<Velocity>(entity);
            Acceleration acceleration = world.GetComponent<Acceleration>(entity);
            
            ref var velRef = ref world.GetModifiableComponent<Velocity>(entity);
            velRef.X += acceleration.X;
            velRef.Y += acceleration.Y;
            velRef.Z += acceleration.Z;
        }
        
        // Update positions with velocity
        var entitiesWithVel = world.GetEntitiesWith<Position, Velocity>();
        foreach (var entity in entitiesWithVel)
        {
            Velocity velocity = world.GetComponent<Velocity>(entity);
            
            ref var posRef = ref world.GetModifiableComponent<Position>(entity);
            posRef.X += velocity.X;
            posRef.Y += velocity.Y;
            posRef.Z += velocity.Z;
        }
    }
}

public class SetValueSystem : SystemHandler
{
    private int valueToSet;
    
    public SetValueSystem(int value)
    {
        this.valueToSet = value;
    }
    
    public override void Execute(WorldInstance world)
    {
        var counters = world.GetModifiableComponents<CounterComponent>();
        for (int i = 0; i < counters.Count; i++)
        {
            counters[i].Value = valueToSet;
        }
    }
}

public class UpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var counters = world.GetModifiableComponents<UpdateCounter>();
        for (int i = 0; i < counters.Count; i++)
        {
            counters[i].Value++;
        }
    }
}

public class FixedUpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var counters = world.GetModifiableComponents<FixedUpdateCounter>();
        for (int i = 0; i < counters.Count; i++)
        {
            counters[i].Value++;
        }
    }
}

public class CleanupSystem : SystemHandler
{
    // Store entity to clean up (simplified for test)
    private Entity? entityToCleanup;
    
    public void SetEntityToCleanup(Entity entity)
    {
        entityToCleanup = entity;
    }
    
    public override void Execute(WorldInstance world)
    {
        // Use GetEntitiesWith to find entities with both Health and Dead components
        var entities = world.GetEntitiesWith<Health, Dead>();
        
        foreach (var entity in entities)
        {
            Health health = world.GetComponent<Health>(entity);
            if (health.Amount <= 0f)
            {
                world.RemoveComponent<Dead>(entity);
            }
        }
        
        // Also handle specific entity if set (for testing)
        if (entityToCleanup.HasValue)
        {
            Entity entity = entityToCleanup.Value;
            if (entity.Has<Health>(world))
            {
                Health health = world.GetComponent<Health>(entity);
                if (health.Amount <= 0f && entity.Has<Dead>(world))
                {
                    world.RemoveComponent<Dead>(entity);
                }
            }
        }
    }
}

public class SpawnSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Use GetEntitiesWith to find entities with Spawner component
        var entities = world.GetEntitiesWith<Spawner>();
        
        foreach (var entity in entities)
        {
            Spawner spawner = world.GetComponent<Spawner>(entity);
            if (spawner.SpawnCount > 0)
            {
                // Create new entity in the same world context
                Entity newEntity = world.CreateEntity();
                
                // Add some component to new entity
                world.AddComponent(newEntity, new TestComponent { Value = 42 });
                
                // Decrement spawn count
                ref var spawnerRef = ref world.GetModifiableComponent<Spawner>(entity);
                spawnerRef.SpawnCount--;
                
                break; // Only spawn one per frame
            }
        }
    }
}

/// <summary>
/// Example TransformSyncSystem for testing Entity ↔ GameObject linking.
/// Synchronizes Position component to Transform.position.
/// </summary>
public class TransformSyncSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.GetEntitiesWith<Position>();

        foreach (var entity in entities)
        {
            Position position = world.GetComponent<Position>(entity);
            GameObject go = world.GetGameObject(entity);

            if (go != null)
            {
                go.transform.position = new Vector3(position.X, position.Y, position.Z);
            }
        }
    }
}
