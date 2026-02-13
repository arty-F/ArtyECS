# ArtyECS

A lightweight, beginner-friendly, zero allocations Entity Component System (ECS) framework for Unity, designed with simplicity and clarity in mind. ArtyECS provides a clean API that makes it easy to get started with ECS architecture, even if you're new to the pattern.

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

### 🛠️ Editor Tools (IN PROGRESS)
- **Visual hierarchy** - see your ECS structure in Unity Hierarchy window
- **Component inspection** - view and edit component values in Play Mode
- **Runtime debugging** - monitor entities, components, and systems in real-time
- **Editor-only** - all tools automatically excluded from builds

## Installation

Requires a version of unity that supports path query parameter for git packages (Unity 2021.3 or later). You can add a reference `https://github.com/arty-F/ArtyECS.git?path=Assets/ArtyECS` to Package Manager.

## Quick Start

ArtyECS makes ECS operations simple and intuitive. Here's how easy it is to get started with the current API:

### 1. Define a Context-based Component

For better compatibility with Unity, components have been renamed to contexts. Contexts are simple classes that inherit from `Context`:

```csharp
using ArtyECS.Core;

public class Health : Context
{
    public float Amount;
}
```

### 2. Create a System

Systems inherit from `SystemHandler` and operate on entities through a `WorldInstance`:

```csharp
public class HealthDecaySystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Game logic here
    }
}
```

### 3. Bootstrap the World and Register Systems

In your scene, create a `MonoBehaviour` that registers systems on startup, similar to the `Assets/Tests/Scripts/MiniGameScenario.cs` sample:

```csharp
public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        World.RegisterSystem(new HealthDecaySystem());
        // Other systems ...
    }
}
```

### 4. Create Entities and Attach Contexts

You typically create entities inside systems or setup code. For example, a simplified spawn system might look like this:

```csharp
public class SimpleSpawnSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Creating entity
        var entity = world.CreateEntity();

        // Adding context to entity
        var health = entity.Add<Health>();

        // Init context values
        health.Amount = 100f;
    }
}
```

That's it! Once systems are registered, ArtyECS will call `Execute` on them each frame, and your game logic will run entirely through ECS.

## Core Concepts

### Worlds

ArtyECS uses a global world by default and can support multiple worlds for more advanced scenarios.

**Global World (default):**
```csharp
// Access global world instance
var globalWorld = World.Global;
```

In most cases you don't need to hold a reference to the world yourself. Systems receive a `WorldInstance` in their `Execute` method:

```csharp
public class ExampleSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        // Use world to create entities, query, etc.
        var entity = world.CreateEntity();
    }
}
```

You can create additional worlds and manage their lifetime through the `World` API, but for most games a single global world is enough.

### Entities

Entities are classes that store their contexts.

**Creating and linking:**
```csharp
// Create entity in a system (using WorldInstance)
var entity = world.CreateEntity();
```

Entities can also be linked to Unity `GameObject` instances. This is how the MiniGame sample spawns enemies:

```csharp
var enemyGameObject = Object.Instantiate(prefab, position, rotation);
var enemy = world.CreateEntity(enemyGameObject);

enemy.Add<Enemy>();
var positionContext = enemy.Add<Position>();
positionContext.X = position.x;
positionContext.Y = position.y;
positionContext.Z = position.z;
```

ArtyECS takes care of cleaning up entities and their contexts when they are destroyed.

### Contexts

Contexts are data holders attached to entities, they are implemented as classes inheriting from `Context`.

**Adding and Removing:**
```csharp
// Add context instances via entity extension methods
var health = entity.Add<Health>();
health.Amount = 100f;

// Remove context
entity.Remove<Health>();
```

**Getting Contexts:**
```csharp
// Get context (throws if not found)
var health = entity.Get<Health>();

// Check if entity has context
if (entity.Have<Health>())
{
    // Entity has Health context
}
```

For configuration data or global state it is often convenient to use **unique contexts**. You attach them once and access them through the world:

```csharp
var playerConfig = new PlayerSpawnConfig { Health = 100f, MoveSpeed = 5f };
var entity = world.CreateEntity();
// Adding a uniq context
entity.AddUniq<PlayerSpawnConfig>(playerConfig);

// Reading a unique context
var config = world.GetUniqContext<PlayerSpawnConfig>();
```

### Systems

Systems contain the logic that processes entities and components. They are regular C# classes that inherit from `SystemHandler` and are registered once at startup.

**Creating Systems:**
```csharp
public class MovementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {

    }
}
```

**Registering Systems:**
```csharp
public class MiniGameScenario : MonoBehaviour
{
    private void Start()
    {
        // System are calls in update queue by default
        World.RegisterSystem(new MovementSystem());
        // But you can choose specific queue
        World.RegisterSystem(new InputSystem(), type: UpdateType.FixedUpdate);
    }
}
```

Systems are then executed automatically by the framework in update or fixed update queues. For setup-style systems you can also execute them manually by calling `mySystem.Execute(World.Global)` once.

### Queries

Queries find entities based on which contexts they have attached. Queries are created from a `WorldInstance` and are composable.

```csharp
// Query entities with Position, MoveDirection and without Dead
var entities = world
    .Query()
    .With<Position>()
    .With<MoveDirection>()
    .Without<Dead>()
    .Execute();

foreach (var entity in entities)
{
    var position = entity.Get<Position>();
    // ...
}
```

You can combine multiple `With<T>()` clauses, and also exclude components with `Without<T>()`. The MiniGame example uses this pattern extensively for spawning, navigation and proximity checks.

## Editor Tools

Editor tools (visual hierarchy, inspectors, performance monitor, etc.) are currently being refactored and are temporarily disabled in the public package.
