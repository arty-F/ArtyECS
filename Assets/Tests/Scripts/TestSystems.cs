using ArtyECS.Core;
using System;

// Test systems for tests
public class TestSystem : SystemHandler
{
    private Action executeAction;
    
    public TestSystem(Action executeAction)
    {
        this.executeAction = executeAction;
    }
    
    public override void Execute()
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
    public override void Execute()
    {
        var counters = ComponentsRegistry.GetModifiableComponents<CounterComponent>();
        using (counters)
        {
            for (int i = 0; i < counters.Count; i++)
            {
                counters[i].Value++;
            }
        }
    }
}

public class MovementSystem : SystemHandler
{
    public override void Execute()
    {
        var positions = ComponentsRegistry.GetModifiableComponents<Position>();
        var velocities = ComponentsRegistry.GetComponents<Velocity>();
        
        using (positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                // Find corresponding velocity (simplified - assumes same index)
                if (i < velocities.Length)
                {
                    positions[i].X += velocities[i].X;
                    positions[i].Y += velocities[i].Y;
                    positions[i].Z += velocities[i].Z;
                }
            }
        }
    }
}

public class HealthSystem : SystemHandler
{
    public override void Execute()
    {
        var healths = ComponentsRegistry.GetModifiableComponents<Health>();
        using (healths)
        {
            for (int i = 0; i < healths.Count; i++)
            {
                healths[i].Amount -= 1f;
            }
        }
    }
}

public class ModifiableHealthSystem : SystemHandler
{
    public override void Execute()
    {
        var healths = ComponentsRegistry.GetModifiableComponents<Health>();
        using (healths)
        {
            for (int i = 0; i < healths.Count; i++)
            {
                healths[i].Amount -= 1f;
            }
        }
    }
}

public class PhysicsSystem : SystemHandler
{
    public override void Execute()
    {
        var velocities = ComponentsRegistry.GetModifiableComponents<Velocity>();
        var accelerations = ComponentsRegistry.GetComponents<Acceleration>();
        
        using (velocities)
        {
            for (int i = 0; i < velocities.Count; i++)
            {
                if (i < accelerations.Length)
                {
                    velocities[i].X += accelerations[i].X;
                    velocities[i].Y += accelerations[i].Y;
                    velocities[i].Z += accelerations[i].Z;
                }
            }
        }
        
        var positions = ComponentsRegistry.GetModifiableComponents<Position>();
        var velocitiesRead = ComponentsRegistry.GetComponents<Velocity>();
        
        using (positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                if (i < velocitiesRead.Length)
                {
                    positions[i].X += velocitiesRead[i].X;
                    positions[i].Y += velocitiesRead[i].Y;
                    positions[i].Z += velocitiesRead[i].Z;
                }
            }
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
    
    public override void Execute()
    {
        var counters = ComponentsRegistry.GetModifiableComponents<CounterComponent>();
        using (counters)
        {
            for (int i = 0; i < counters.Count; i++)
            {
                counters[i].Value = valueToSet;
            }
        }
    }
}

public class UpdateCounterSystem : SystemHandler
{
    public override void Execute()
    {
        var counters = ComponentsRegistry.GetModifiableComponents<UpdateCounter>();
        using (counters)
        {
            for (int i = 0; i < counters.Count; i++)
            {
                counters[i].Value++;
            }
        }
    }
}

public class FixedUpdateCounterSystem : SystemHandler
{
    public override void Execute()
    {
        var counters = ComponentsRegistry.GetModifiableComponents<FixedUpdateCounter>();
        using (counters)
        {
            for (int i = 0; i < counters.Count; i++)
            {
                counters[i].Value++;
            }
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
    
    public override void Execute()
    {
        // Simplified: remove Dead component from entity with Health <= 0
        // In a real system, we'd iterate through all entities with Health and Dead components
        if (entityToCleanup.HasValue)
        {
            var health = ComponentsRegistry.GetComponent<Health>(entityToCleanup.Value);
            if (health.HasValue && health.Value.Amount <= 0f)
            {
                ComponentsRegistry.RemoveComponent<Dead>(entityToCleanup.Value);
            }
        }
        else
        {
            // Fallback: try to find and remove Dead from any entity with Health <= 0
            // This is a limitation - we can't easily iterate entities in current API
            // For this test, we'll use a workaround: check if we can find entities with both components
            var healthAndDead = ComponentsRegistry.GetComponents<Health, Dead>();
            // Note: We can't get Entity from component in current API
            // This demonstrates a limitation that would need to be addressed in real implementation
        }
    }
}

public class SpawnSystem : SystemHandler
{
    public override void Execute()
    {
        var spawners = ComponentsRegistry.GetComponents<Spawner>();
        
        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i].SpawnCount > 0)
            {
                // Create new entity
                Entity newEntity = World.CreateEntity();
                
                // Add some component to new entity (optional)
                ComponentsRegistry.AddComponent(newEntity, new TestComponent { Value = 42 });
                
                // Decrement spawn count
                var modifiableSpawners = ComponentsRegistry.GetModifiableComponents<Spawner>();
                using (modifiableSpawners)
                {
                    if (i < modifiableSpawners.Count)
                    {
                        modifiableSpawners[i].SpawnCount--;
                    }
                }
                break; // Only spawn one per frame
            }
        }
    }
}

