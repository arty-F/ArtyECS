# ArtyECS

A lightweight, beginner-friendly Entity Component System (ECS) framework for Unity, designed with simplicity and clarity in mind. ArtyECS provides a clean API that makes it easy to get started with ECS architecture, even if you're new to the pattern.

## Features

### 🎯 Simple and Intuitive API
- **Clear method names** that express their purpose
- **No complex registration** - systems are just classes you instantiate
- **Straightforward component management** - add, get, remove with simple calls
- **Beginner-friendly** - designed to be approachable for developers new to ECS

### 🔧 Code-First Approach
- **Maximum functionality through code** - minimal Unity Inspector dependency
- **No ScriptableObject configuration** - everything happens in code
- **Automatic initialization** - All created automatically when needed

### 🎮 Unity Integration
- **Hybrid approach** - works alongside MonoBehaviour, doesn't replace it
- **Entity ↔ GameObject linking** - easily connect ECS entities to Unity GameObjects
- **Scene persistence** - ECS World persists across Unity scene changes
- **Multiple worlds support** - global world by default, create scoped worlds when needed

### 🛠️ Editor Tools
- **Visual hierarchy** - see your ECS structure in Unity Hierarchy window
- **Component inspection** - view and edit component values in Play Mode
- **Runtime debugging** - monitor entities, components, and systems in real-time
- **Editor-only** - all tools automatically excluded from builds

### 📊 Monitoring Tools
- **Performance metrics** - track system execution times
- **Query performance** - monitor component query efficiency
- **Memory usage** - view component storage statistics
- **Allocation tracking** - identify memory allocation patterns

## Installation

Requires a version of unity that supports path query parameter for git packages (Unity 2021.3 or later). You can add a reference https://github.com/arty-F/ArtyECS.git?path=Assets/ArtyECS to Package Manager.

## Quick Start

ArtyECS makes ECS operations simple and intuitive. Here's how easy it is to get started:

### 1. Define a Component

Components are simple structs that implement `IComponent`:

```csharp
using ArtyECS.Core;

public struct Health : IComponent
{
    public float Amount;
}
```

### 2. Create Entities and Add Components

```csharp
// Create an entity
var entity = World.CreateEntity();

// Add a component
World.AddComponent(entity, new Health { Amount = 100 });
```

### 3. Create a System with Queries

Systems use queries to find entities. Here's a system that finds entities with `Health` but without `Dead` component, and modifies the health value:

```csharp
using ArtyECS.Core;

public class HealthSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Query: entities with Health component, but without Dead component
        var entities = world.Query()
            .With<Health>()
            .Without<Dead>()
            .Execute();
        
        foreach (var entity in entities)
        {
            // Get modifiable reference to component
            ref var health = ref world.GetModifiableComponent<Health>(entity);
            
            // Modify component value directly
            health.Amount -= 1f;
        }
    }
}
```

### 4. Register the System

```csharp
var healthSystem = new HealthSystem();
World.AddToUpdate(healthSystem);
```

That's it! The system will automatically execute every frame, finding all living entities and reducing their health.

## Core Concepts

### Worlds

ArtyECS supports multiple worlds. By default, all operations use the global world. You can also create named local worlds for isolation.

**Global World (default):**
```csharp
// All World static methods use global world automatically
var entity = World.CreateEntity();
World.AddComponent(entity, new Health { Amount = 100 });

// Access global world explicitly
var globalWorld = World.GlobalWorld;
```

**Local Worlds:**
```csharp
// Get or create a named world
var localWorld = World.GetOrCreate("MyWorld");

// Check if world exists
if (World.Exists("MyWorld"))
{
    // World exists
}

// Get all worlds
var allWorlds = World.GetAllWorlds();

// Destroy local world (cannot destroy global world)
World.Destroy(localWorld);
```

**World Methods (for global world):**
- `World.GetOrCreate(string name = null)` - Get or create world (null = global)
- `World.GlobalWorld` - Access global world instance
- `World.Exists(string name)` - Check if world exists
- `World.GetAllWorlds()` - Get all active worlds
- `World.Destroy(WorldInstance world)` - Destroy local world
- `World.ClearAllECSState()` - Clear all worlds

**Using WorldInstance:**
All `World` static methods are also available as instance methods on `WorldInstance`. Use `World` for the global world, or `WorldInstance` for a specific world:

```csharp
// Using World (global world)
var entity = World.CreateEntity();
var health = World.GetComponent<Health>(entity);

// Using WorldInstance (specific world)
var localWorld = World.GetOrCreate("MyWorld");
var entity = localWorld.CreateEntity();
var health = localWorld.GetComponent<Health>(entity);
```

### Entities

Entities are lightweight identifiers (structs with `Id` and `Generation` fields). They don't store data themselves - they're just handles used to access components.

**Creating and Destroying:**
```csharp
// Create entity (in global world)
var entity = World.CreateEntity();

// Create entity in specific world
var entity = localWorld.CreateEntity();

// Create entity linked to GameObject
var entity = World.CreateEntity(gameObject);
var entity = localWorld.CreateEntity(gameObject);

// Destroy entity (automatically removes all components, unlinks and destroys linked GameObject if exists)
World.DestroyEntity(entity);
localWorld.DestroyEntity(entity);
```

**Validation:**
```csharp
// Check if entity is valid
if (World.IsEntityValid(entity))
{
    // Entity is valid
}
```

**Cloning:**
```csharp
// Clone entity with all components
var clonedEntity = World.CloneEntity(sourceEntity);
```

**Entity ↔ GameObject Linking:**
```csharp
// Create entity linked to GameObject
var entity = World.CreateEntity(gameObject);

// Get GameObject from entity
var go = World.GetGameObject(entity);

// Get entity from GameObject
var entity = World.GetEntity(gameObject);
```

**Entity Methods:**
- `World.CreateEntity()` / `worldInstance.CreateEntity()` - Create entity
- `World.CreateEntity(GameObject)` / `worldInstance.CreateEntity(GameObject)` - Create and link
- `World.DestroyEntity(Entity)` / `worldInstance.DestroyEntity(Entity)` - Destroy entity
- `World.IsEntityValid(Entity)` / `worldInstance.IsEntityValid(Entity)` - Check validity
- `World.GetGameObject(Entity)` / `worldInstance.GetGameObject(Entity)` - Get linked GameObject
- `World.GetEntity(GameObject)` / `worldInstance.GetEntity(GameObject)` - Get linked entity
- `World.GetAllEntities()` / `worldInstance.GetAllEntities()` - Get all entities

**Entity Extension Methods:**
- `entity.Get<T>(WorldInstance world = null)` - Get component (uses global world if null)
- `entity.Has<T>(WorldInstance world = null)` - Check if has component
- `entity.AddComponent<T>(T component, WorldInstance world = null)` - Add component
- `entity.RemoveComponent<T>(WorldInstance world = null)` - Remove component

### Components

Components are structs that store data. Each entity can have at most one component of each type.

**Adding and Removing:**
```csharp
// Add component
World.AddComponent(entity, new Health { Amount = 100 });
localWorld.AddComponent(entity, new Health { Amount = 100 });
entity.AddComponent(new Health { Amount = 100 }); // Extension method

// Remove component
World.RemoveComponent<Health>(entity);
localWorld.RemoveComponent<Health>(entity);
entity.RemoveComponent<Health>(); // Extension method
```

**Getting Components:**
```csharp
// Get component (throws ComponentNotFoundException if not found)
var health = World.GetComponent<Health>(entity);
var health = localWorld.GetComponent<Health>(entity);
var health = entity.Get<Health>(); // Extension method

// Get modifiable component (for direct modification)
ref var health = ref World.GetModifiableComponent<Health>(entity);
ref var health = ref localWorld.GetModifiableComponent<Health>(entity);
//todo ref var health = ref entity.GetModifiable<Health>();

// Check if entity has component
if (entity.Has<Health>())
{
    // Entity has Health component
}
```

**Getting All Components of Type:**
```csharp
// Read-only access
var healths = World.GetComponents<Health>();
foreach (ref readonly var health in healths)
{
    // Process health
}

// Modifiable access
var healths = World.GetModifiableComponents<Health>();
for (int i = 0; i < healths.Count; i++)
{
    ref var health = ref healths[i];
    health.Amount -= 1f; // Modify directly
}
```

**Component Methods:**
- `World.AddComponent<T>(Entity, T)` / `worldInstance.AddComponent<T>(Entity, T)` - Add component
- `World.GetComponent<T>(Entity)` / `worldInstance.GetComponent<T>(Entity)` - Get component
- `World.GetModifiableComponent<T>(Entity)` / `worldInstance.GetModifiableComponent<T>(Entity)` - Get modifiable ref
- `World.RemoveComponent<T>(Entity)` / `worldInstance.RemoveComponent<T>(Entity)` - Remove component
- `World.GetComponents<T>()` / `worldInstance.GetComponents<T>()` - Get all components (read-only)
- `World.GetModifiableComponents<T>()` / `worldInstance.GetModifiableComponents<T>()` - Get all components (modifiable)
- `World.GetAllComponentInfos(Entity)` / `worldInstance.GetAllComponentInfos(Entity)` - Get all component info
- `World.CloneEntity(Entity)` / `worldInstance.CloneEntity(Entity)` - Clone entity

### Systems

Systems contain the logic that processes entities and components. They execute in a deterministic order within their queue. Systems are tied to Unity's Update/FixedUpdate lifecycle - Update systems execute every frame, FixedUpdate systems execute every fixed timestep.

**Creating Systems:**
```csharp
public class HealthSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.GetEntitiesWith<Health>();
        
        foreach (var entity in entities)
        {
            ref var health = ref world.GetModifiableComponent<Health>(entity);
            health.Amount -= 1f;
        }
    }
}
```

**Registering Systems:**
```csharp
var healthSystem = new HealthSystem();

// Add to Update queue (executes every frame)
World.AddToUpdate(healthSystem);
localWorld.AddToUpdate(healthSystem);
healthSystem.AddToUpdate(); // Extension method (uses global world)
healthSystem.AddToUpdate(localWorld); // Extension method (specific world)

// Add to Update with execution order (lower values execute first)
World.AddToUpdate(healthSystem, order: 100);
healthSystem.AddToUpdate(order: 100); // Extension method

// Add to FixedUpdate queue (executes every fixed timestep)
World.AddToFixedUpdate(healthSystem);
healthSystem.AddToFixedUpdate(); // Extension method

// Add to FixedUpdate with execution order
World.AddToFixedUpdate(healthSystem, order: 50);
healthSystem.AddToFixedUpdate(order: 50); // Extension method
```

**Manual Execution:**
```csharp
// Execute system once immediately (not added to queue)
World.ExecuteOnce(healthSystem);
localWorld.ExecuteOnce(healthSystem);
healthSystem.ExecuteOnce(); // Extension method

// Manually execute all systems in queue
// Note: These methods execute systems immediately, in addition to Unity's automatic execution. Systems will be called twice per frame if you call these manually.
World.ExecuteUpdate(); // Execute all Update systems
World.ExecuteFixedUpdate(); // Execute all FixedUpdate systems
```

**Removing Systems:**
```csharp
// Remove from Update queue
World.RemoveFromUpdate(healthSystem);

// Remove from FixedUpdate queue
World.RemoveFromFixedUpdate(healthSystem);
```

**Inspecting Queues:**
```csharp
// Get all systems in Update queue
var updateSystems = World.GetUpdateQueue();

// Get all systems in FixedUpdate queue
var fixedUpdateSystems = World.GetFixedUpdateQueue();
```

**System Methods:**
- `World.AddToUpdate(SystemHandler)` / `worldInstance.AddToUpdate(SystemHandler)` - Add to Update queue
- `World.AddToUpdate(SystemHandler, int order)` / `worldInstance.AddToUpdate(SystemHandler, int order)` - Add with order
- `World.AddToFixedUpdate(SystemHandler)` / `worldInstance.AddToFixedUpdate(SystemHandler)` - Add to FixedUpdate queue
- `World.AddToFixedUpdate(SystemHandler, int order)` / `worldInstance.AddToFixedUpdate(SystemHandler, int order)` - Add with order
- `World.ExecuteOnce(SystemHandler)` / `worldInstance.ExecuteOnce(SystemHandler)` - Execute once
- `World.ExecuteUpdate()` / `worldInstance.ExecuteUpdate()` - Execute Update queue
- `World.ExecuteFixedUpdate()` / `worldInstance.ExecuteFixedUpdate()` - Execute FixedUpdate queue
- `World.RemoveFromUpdate(SystemHandler)` / `worldInstance.RemoveFromUpdate(SystemHandler)` - Remove from Update
- `World.RemoveFromFixedUpdate(SystemHandler)` / `worldInstance.RemoveFromFixedUpdate(SystemHandler)` - Remove from FixedUpdate
- `World.GetUpdateQueue()` / `worldInstance.GetUpdateQueue()` - Get Update queue
- `World.GetFixedUpdateQueue()` / `worldInstance.GetFixedUpdateQueue()` - Get FixedUpdate queue

**System Extension Methods:**
- `system.AddToUpdate(WorldInstance world = null)` - Add to Update
- `system.AddToUpdate(int order, WorldInstance world = null)` - Add to Update with order
- `system.AddToFixedUpdate(WorldInstance world = null)` - Add to FixedUpdate
- `system.AddToFixedUpdate(int order, WorldInstance world = null)` - Add to FixedUpdate with order
- `system.ExecuteOnce(WorldInstance world = null)` - Execute once

### Queries

Queries find entities based on component presence. ArtyECS supports queries with up to 3 component types using direct methods, or unlimited types using QueryBuilder.

**Simple Queries (up to 3 types):**
```csharp
// Entities with single component
var entities = World.GetEntitiesWith<Health>();
var entities = localWorld.GetEntitiesWith<Health>();

// Entities with two components
var entities = World.GetEntitiesWith<Health, Position>();

// Entities with three components
var entities = World.GetEntitiesWith<Health, Position, Velocity>();

// Entities without single component
var entities = World.GetEntitiesWithout<Dead>();

// Entities without two components (without T1, T2)
var entities = World.GetEntitiesWithout<Dead, Destroyed>();

// Entities without three components (without T1, T2, T3)
var entities = World.GetEntitiesWithout<Dead, Destroyed, Removed>();
```

**Composable Queries (QueryBuilder):**
```csharp
// Query with multiple With and Without conditions
var entities = World.Query()
    .With<Health>()
    .With<Position>()
    .Without<Dead>()
    //...
    .Execute();
```

**Query Methods:**
- `World.GetEntitiesWith<T1>()` / `worldInstance.GetEntitiesWith<T1>()` - Entities with T1
- `World.GetEntitiesWith<T1, T2>()` / `worldInstance.GetEntitiesWith<T1, T2>()` - Entities with T1 and T2
- `World.GetEntitiesWith<T1, T2, T3>()` / `worldInstance.GetEntitiesWith<T1, T2, T3>()` - Entities with T1, T2, and T3
- `World.GetEntitiesWithout<T1>()` / `worldInstance.GetEntitiesWithout<T1>()` - Entities without T1
- `World.GetEntitiesWithout<T1, T2>()` / `worldInstance.GetEntitiesWithout<T1, T2>()` - Entities without T1 or T2
- `World.GetEntitiesWithout<T1, T2, T3>()` / `worldInstance.GetEntitiesWithout<T1, T2, T3>()` - Entities without T1, T2, or T3
- `World.Query()` / `worldInstance.Query()` - Create QueryBuilder for composable queries

## Editor Tools

### Visual Hierarchy

When you enter Play Mode, ArtyECS automatically creates a visual hierarchy in the Unity Hierarchy window:

- **ArtyEcs** (root)
  - **Global** (world)
    - **Entities** (container)
      - **Entity_123** (entity)
        - Component displays
    - **Systems** (container)
      - **Update** (queue)
        - **MovementSystem** (system)
      - **FixedUpdate** (queue)
        - **PhysicsSystem** (system)


<img width="1142" height="581" alt="s0" src="https://github.com/user-attachments/assets/6f6fe8d3-c40d-41e3-b8c6-58bb405aa9d1" />

### Entity Inspection

In Play Mode, you can create a new entity from inspector:

<img width="801" height="635" alt="s1" src="https://github.com/user-attachments/assets/ed283dd8-ab21-4632-a265-644b174a845f" />

### Component Inspection

In Play Mode, you can:
- **View component values**
- **Edit component values in real time**
- **Add or remove components**
- **Monitor changes**

<img width="802" height="805" alt="s2" src="https://github.com/user-attachments/assets/1d0652ca-0f2d-4374-99f5-96f363fef0d5" />

### System Inspection

In Play Mode, you can add any system to queue:

<img width="801" height="655" alt="s4" src="https://github.com/user-attachments/assets/661bd8c7-68fb-4a82-8e86-e3be28c662c3" />

Also you can remove specific sysem from a queue:

<img width="803" height="638" alt="s3" src="https://github.com/user-attachments/assets/f5c2a3c0-bfa9-4999-b6ef-d14875c3bccd" />

### Performance Monitoring

ArtyECS includes built-in performance monitoring tools accessible through the Unity Editor.

### Opening the Performance Monitor

Navigate to: **Window → ArtyECS → Performance Monitor** → Enable Monitoring (in monitoring window)

<img width="816" height="584" alt="s5" src="https://github.com/user-attachments/assets/ed078243-47cb-4152-9e6e-cfd4b7aafcc0" />

### Available Metrics

The Performance Monitor displays:

1. **System Execution Times**
   - Execution time per system
   - Total execution time per frame
   - System execution order

2. **Query Performance**
   - Query execution times
   - Number of entities processed
   - Query frequency

3. **Memory Usage**
   - Component storage statistics
   - Entity pool usage
   - Memory allocation patterns

4. **Allocation Tracking**
   - Allocation events per frame
   - Allocation sources
   - Memory growth over time

<img width="816" height="864" alt="s6" src="https://github.com/user-attachments/assets/5b7f98b4-fd1b-4117-82bf-4f3906148791" />

### Using the Monitor

- **Auto-refresh**: Toggle to automatically update metrics
- **Manual refresh**: Click refresh button to update immediately
- **World filtering**: Select specific world to monitor (if multiple worlds exist)
- **Throttled updates**: Metrics update every 1 seconds by default
