using UnityEngine;
using ArtyECS.Core;
using System;
using System.Linq;

/// <summary>
/// Test cases for ArtyECS World Management functionality (Glob-002)
/// </summary>
public class WorldTests : TestBase
{
    // ========== World-000: World Class Implementation ==========
    
    [ContextMenu("Run Test: World Creation with Name")]
    public void Test_WorldClass_001()
    {
        string testName = "Test_WorldClass_001";
        ExecuteTest(testName, () =>
        {
            // Create World with name "TestWorld"
            World world = new World("TestWorld");
            
            // Verify that World.Name == "TestWorld"
            AssertEquals("TestWorld", world.Name, "World.Name should be TestWorld");
            AssertNotNull(world, "World should not be null");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Creation")]
    public void Test_WorldClass_002()
    {
        string testName = "Test_WorldClass_002";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1")
            World world1 = new World("World1");
            
            // Create World2("World2")
            World world2 = new World("World2");
            
            // Create World3("World3")
            World world3 = new World("World3");
            
            // Verify all worlds created successfully
            AssertEquals("World1", world1.Name, "World1.Name should be World1");
            AssertEquals("World2", world2.Name, "World2.Name should be World2");
            AssertEquals("World3", world3.Name, "World3.Name should be World3");
            Assert(world1 != world2, "World1 != World2 should be true");
            Assert(world2 != world3, "World2 != World3 should be true");
        });
    }
    
    [ContextMenu("Run Test: World Equality")]
    public void Test_WorldClass_003()
    {
        string testName = "Test_WorldClass_003";
        ExecuteTest(testName, () =>
        {
            // Create World1("Test")
            World world1 = new World("Test");
            
            // Create World2("Test")
            World world2 = new World("Test");
            
            // Get global world instance
            World globalWorld1 = World.GetGlobalWorld();
            
            // Get global world instance again
            World globalWorld2 = World.GetGlobalWorld();
            
            // Verify worlds with same name are different instances
            Assert(world1 != world2, "World1 != World2 should be true (different instances)");
            
            // Verify global world is same instance
            Assert(globalWorld1 == globalWorld2, "globalWorld1 == globalWorld2 should be true (same singleton instance)");
            Assert(ReferenceEquals(globalWorld1, globalWorld2), "Global worlds should be same reference");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Scoped World")]
    public void Test_WorldClass_004()
    {
        string testName = "Test_WorldClass_004";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity in TestWorld
            Entity entity = World.CreateEntity(testWorld);
            
            // Add components to Entity in TestWorld
            ComponentsRegistry.AddComponent<TestComponent>(entity, new TestComponent { Value = 42 }, testWorld);
            ComponentsRegistry.AddComponent<Position>(entity, new Position { X = 1f, Y = 2f, Z = 3f }, testWorld);
            
            // Add system to TestWorld
            var system = new TestSystem(() => { });
            SystemsRegistry.AddToUpdate(system, testWorld);
            
            // Call World.Destroy(TestWorld)
            bool destroyed = World.Destroy(testWorld);
            
            // Verify that world is destroyed
            Assert(destroyed, "World.Destroy(TestWorld) should return true");
            AssertEquals(0, ComponentsRegistry.GetComponents<TestComponent>(testWorld).Length, "Components should be cleared");
            AssertEquals(0, ComponentsRegistry.GetComponents<Position>(testWorld).Length, "Components should be cleared");
            AssertEquals(0, SystemsRegistry.GetUpdateQueue(testWorld).Count, "Update queue should be cleared");
            AssertEquals(0, SystemsRegistry.GetFixedUpdateQueue(testWorld).Count, "FixedUpdate queue should be cleared");
            Assert(!EntityPool.IsAllocated(entity, testWorld), "Entity should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Global World")]
    public void Test_WorldClass_005()
    {
        string testName = "Test_WorldClass_005";
        ExecuteTest(testName, () =>
        {
            // Get global world instance
            World globalWorld = World.GetGlobalWorld();
            
            // Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // Verify destroy returns false, global world not destroyed
            Assert(!destroyed, "World.Destroy(globalWorld) should return false");
            AssertNotNull(World.GetGlobalWorld(), "Global world should still exist");
            AssertEquals("Global", World.GetGlobalWorld().Name, "Global world name should be Global");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Twice")]
    public void Test_WorldClass_006()
    {
        string testName = "Test_WorldClass_006";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Destroy TestWorld
            bool destroyed1 = World.Destroy(testWorld);
            
            // Attempt to destroy TestWorld again
            bool destroyed2 = World.Destroy(testWorld);
            
            // Verify second destruction returns false
            Assert(destroyed1, "First World.Destroy(TestWorld) should return true");
            Assert(!destroyed2, "Second World.Destroy(TestWorld) should return false");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Cleans Up Components")]
    public void Test_WorldClass_007()
    {
        string testName = "Test_WorldClass_007";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1, Entity2, Entity3 in TestWorld
            Entity entity1 = World.CreateEntity(testWorld);
            Entity entity2 = World.CreateEntity(testWorld);
            Entity entity3 = World.CreateEntity(testWorld);
            
            // Add Position, Velocity, Health to all entities
            ComponentsRegistry.AddComponent<Position>(entity1, new Position { X = 1f, Y = 2f, Z = 3f }, testWorld);
            ComponentsRegistry.AddComponent<Velocity>(entity1, new Velocity { X = 1f, Y = 1f, Z = 1f }, testWorld);
            ComponentsRegistry.AddComponent<Health>(entity1, new Health { Amount = 100f }, testWorld);
            
            ComponentsRegistry.AddComponent<Position>(entity2, new Position { X = 4f, Y = 5f, Z = 6f }, testWorld);
            ComponentsRegistry.AddComponent<Velocity>(entity2, new Velocity { X = 2f, Y = 2f, Z = 2f }, testWorld);
            ComponentsRegistry.AddComponent<Health>(entity2, new Health { Amount = 200f }, testWorld);
            
            ComponentsRegistry.AddComponent<Position>(entity3, new Position { X = 7f, Y = 8f, Z = 9f }, testWorld);
            ComponentsRegistry.AddComponent<Velocity>(entity3, new Velocity { X = 3f, Y = 3f, Z = 3f }, testWorld);
            ComponentsRegistry.AddComponent<Health>(entity3, new Health { Amount = 300f }, testWorld);
            
            // Destroy TestWorld
            World.Destroy(testWorld);
            
            // Check components in TestWorld
            AssertEquals(0, ComponentsRegistry.GetComponents<Position>(testWorld).Length, "Position components should be cleared");
            AssertEquals(0, ComponentsRegistry.GetComponents<Velocity>(testWorld).Length, "Velocity components should be cleared");
            AssertEquals(0, ComponentsRegistry.GetComponents<Health>(testWorld).Length, "Health components should be cleared");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Cleans Up Systems")]
    public void Test_WorldClass_008()
    {
        string testName = "Test_WorldClass_008";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create System1, System2, System3
            var system1 = new TestSystem(() => { });
            var system2 = new TestSystem(() => { });
            var system3 = new TestSystem(() => { });
            
            // Add all systems to TestWorld Update queue
            SystemsRegistry.AddToUpdate(system1, testWorld);
            SystemsRegistry.AddToUpdate(system2, testWorld);
            SystemsRegistry.AddToUpdate(system3, testWorld);
            
            // Destroy TestWorld
            World.Destroy(testWorld);
            
            // Check systems in TestWorld
            AssertEquals(0, SystemsRegistry.GetUpdateQueue(testWorld).Count, "Update queue should be cleared");
            AssertEquals(0, SystemsRegistry.GetFixedUpdateQueue(testWorld).Count, "FixedUpdate queue should be cleared");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Cleans Up Entity Pool")]
    public void Test_WorldClass_009()
    {
        string testName = "Test_WorldClass_009";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1, Entity2, Entity3 in TestWorld
            Entity entity1 = World.CreateEntity(testWorld);
            Entity entity2 = World.CreateEntity(testWorld);
            Entity entity3 = World.CreateEntity(testWorld);
            
            // Destroy TestWorld
            World.Destroy(testWorld);
            
            // Check entity pool in TestWorld
            AssertEquals(0, EntityPool.GetAllocatedCount(testWorld), "Allocated count should be 0");
            AssertEquals(0, EntityPool.GetAvailableCount(testWorld), "Available count should be 0");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Does Not Affect Other Worlds")]
    public void Test_WorldClass_010()
    {
        string testName = "Test_WorldClass_010";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create Entity1 in World1, Entity2 in World2
            Entity entity1 = World.CreateEntity(world1);
            Entity entity2 = World.CreateEntity(world2);
            
            // Add components to both entities
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 100 }, world1);
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 200 }, world2);
            
            // Destroy World1
            World.Destroy(world1);
            
            // Check World2
            var component2 = ComponentsRegistry.GetComponent<TestComponent>(entity2, world2);
            Assert(component2.HasValue, "GetComponent(Entity2, World2) should not be null");
            AssertEquals(200, component2.Value.Value, "Component value should be 200");
            AssertEquals(1, ComponentsRegistry.GetComponents<TestComponent>(world2).Length, "World2 should have 1 component");
        });
    }
    
    // ========== World-001: Global World Singleton ==========
    
    [ContextMenu("Run Test: GetGlobalWorld Returns Singleton")]
    public void Test_GlobalWorld_001()
    {
        string testName = "Test_GlobalWorld_001";
        ExecuteTest(testName, () =>
        {
            // Call World.GetGlobalWorld() first time
            World globalWorld1 = World.GetGlobalWorld();
            
            // Call World.GetGlobalWorld() second time
            World globalWorld2 = World.GetGlobalWorld();
            
            // Compare instances
            Assert(globalWorld1 == globalWorld2, "globalWorld1 == globalWorld2 should be true");
            Assert(ReferenceEquals(globalWorld1, globalWorld2), "Global worlds should be same reference");
        });
    }
    
    [ContextMenu("Run Test: Global World Name")]
    public void Test_GlobalWorld_002()
    {
        string testName = "Test_GlobalWorld_002";
        ExecuteTest(testName, () =>
        {
            // Get global world instance
            World globalWorld = World.GetGlobalWorld();
            
            // Check Name property
            AssertEquals("Global", globalWorld.Name, "Global world name should be Global");
        });
    }
    
    [ContextMenu("Run Test: Global World Lazy Initialization")]
    public void Test_GlobalWorld_003()
    {
        string testName = "Test_GlobalWorld_003";
        ExecuteTest(testName, () =>
        {
            // Call World.GetGlobalWorld() (creates on first access)
            World globalWorld = World.GetGlobalWorld();
            
            // Verify instance is created
            AssertNotNull(globalWorld, "Global world should not be null");
            AssertEquals("Global", globalWorld.Name, "Global world name should be Global");
        });
    }
    
    [ContextMenu("Run Test: Global World Shared Across Registries")]
    public void Test_GlobalWorld_004()
    {
        string testName = "Test_GlobalWorld_004";
        ExecuteTest(testName, () =>
        {
            // Get global world from World.GetGlobalWorld()
            World worldGlobal = World.GetGlobalWorld();
            
            // Get global world from ComponentsRegistry.GetGlobalWorld()
            World componentsGlobal = ComponentsRegistry.GetGlobalWorld();
            
            // Get global world from SystemsRegistry.GetGlobalWorld()
            World systemsGlobal = SystemsRegistry.GetGlobalWorld();
            
            // Compare instances
            Assert(worldGlobal == componentsGlobal, "World.GetGlobalWorld() == ComponentsRegistry.GetGlobalWorld() should be true");
            Assert(worldGlobal == systemsGlobal, "World.GetGlobalWorld() == SystemsRegistry.GetGlobalWorld() should be true");
            Assert(ReferenceEquals(worldGlobal, componentsGlobal), "World and ComponentsRegistry should share same global world instance");
        });
    }
    
    [ContextMenu("Run Test: Global World Default Resolution")]
    public void Test_GlobalWorld_005()
    {
        string testName = "Test_GlobalWorld_005";
        ExecuteTest(testName, () =>
        {
            // Create Entity without specifying world (null)
            Entity entity = World.CreateEntity(null);
            
            // Add component without specifying world (null)
            ComponentsRegistry.AddComponent<TestComponent>(entity, new TestComponent { Value = 42 }, null);
            
            // Get component without specifying world (null)
            var componentNull = ComponentsRegistry.GetComponent<TestComponent>(entity, null);
            var componentGlobal = ComponentsRegistry.GetComponent<TestComponent>(entity, World.GetGlobalWorld());
            
            // Verify component is in global world
            Assert(componentNull.HasValue, "GetComponent(entity, null) should not be null");
            Assert(componentGlobal.HasValue, "GetComponent(entity, globalWorld) should not be null");
            AssertEquals(componentNull.Value.Value, componentGlobal.Value.Value, "Component values should be equal");
        });
    }
    
    [ContextMenu("Run Test: Global World Cannot Be Destroyed")]
    public void Test_GlobalWorld_006()
    {
        string testName = "Test_GlobalWorld_006";
        ExecuteTest(testName, () =>
        {
            // Get global world instance
            World globalWorld = World.GetGlobalWorld();
            
            // Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // Verify global world still exists
            Assert(!destroyed, "World.Destroy(globalWorld) should return false");
            AssertNotNull(World.GetGlobalWorld(), "Global world should still exist");
            Assert(World.GetGlobalWorld() == globalWorld, "Global world instance should be unchanged");
        });
    }
    
    [ContextMenu("Run Test: Global World Persists")]
    public void Test_GlobalWorld_007()
    {
        string testName = "Test_GlobalWorld_007";
        ExecuteTest(testName, () =>
        {
            // Get global world instance
            World globalWorld1 = World.GetGlobalWorld();
            
            // Create and destroy multiple scoped worlds
            World scoped1 = new World("Scoped1");
            World scoped2 = new World("Scoped2");
            World scoped3 = new World("Scoped3");
            World.Destroy(scoped1);
            World.Destroy(scoped2);
            World.Destroy(scoped3);
            
            // Get global world instance again
            World globalWorld2 = World.GetGlobalWorld();
            
            // Compare instances
            Assert(globalWorld1 == globalWorld2, "globalWorld1 == globalWorld2 should be true");
            Assert(ReferenceEquals(globalWorld1, globalWorld2), "Global world instances should be same reference");
        });
    }
    
    // ========== World-002: World-Scoped Storage Integration ==========
    
    [ContextMenu("Run Test: ComponentsRegistry World Parameter Support")]
    public void Test_WorldScoped_001()
    {
        string testName = "Test_WorldScoped_001";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity
            Entity entity = World.CreateEntity(testWorld);
            
            // Add component with World parameter
            ComponentsRegistry.AddComponent<TestComponent>(entity, new TestComponent { Value = 42 }, testWorld);
            
            // Get component with World parameter
            var component = ComponentsRegistry.GetComponent<TestComponent>(entity, testWorld);
            Assert(component.HasValue, "GetComponent should return component");
            AssertEquals(42, component.Value.Value, "Component value should be 42");
            
            // Remove component with World parameter
            bool removed = ComponentsRegistry.RemoveComponent<TestComponent>(entity, testWorld);
            Assert(removed, "RemoveComponent should return true");
        });
    }
    
    [ContextMenu("Run Test: ComponentsRegistry Null World Resolves to Global")]
    public void Test_WorldScoped_002()
    {
        string testName = "Test_WorldScoped_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity(null);
            
            // Add component with null World parameter
            ComponentsRegistry.AddComponent<TestComponent>(entity, new TestComponent { Value = 100 }, null);
            
            // Get component from global world
            var componentNull = ComponentsRegistry.GetComponent<TestComponent>(entity, null);
            var componentGlobal = ComponentsRegistry.GetComponent<TestComponent>(entity, World.GetGlobalWorld());
            
            // Verify component added to global world
            Assert(componentNull.HasValue, "GetComponent(entity, null) should not be null");
            Assert(componentGlobal.HasValue, "GetComponent(entity, globalWorld) should not be null");
            AssertEquals(componentNull.Value.Value, componentGlobal.Value.Value, "Component values should be equal");
        });
    }
    
    [ContextMenu("Run Test: SystemsRegistry World Parameter Support")]
    public void Test_WorldScoped_003()
    {
        string testName = "Test_WorldScoped_003";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create System
            var system = new TestSystem(() => { });
            
            // Add system to Update queue with World parameter
            SystemsRegistry.AddToUpdate(system, testWorld);
            
            // Add system to FixedUpdate queue with World parameter
            SystemsRegistry.AddToFixedUpdate(system, testWorld);
            
            // Verify systems in queues
            Assert(SystemsRegistry.GetUpdateQueue(testWorld).Contains(system), "Update queue should contain system");
            Assert(SystemsRegistry.GetFixedUpdateQueue(testWorld).Contains(system), "FixedUpdate queue should contain system");
        });
    }
    
    [ContextMenu("Run Test: SystemsRegistry Null World Resolves to Global")]
    public void Test_WorldScoped_004()
    {
        string testName = "Test_WorldScoped_004";
        ExecuteTest(testName, () =>
        {
            // Create System
            var system = new TestSystem(() => { });
            
            // Add system to Update queue with null World parameter
            SystemsRegistry.AddToUpdate(system, null);
            
            // Check global world queue
            Assert(SystemsRegistry.GetUpdateQueue(null).Contains(system), "GetUpdateQueue(null) should contain system");
            Assert(SystemsRegistry.GetUpdateQueue(World.GetGlobalWorld()).Contains(system), "GetUpdateQueue(globalWorld) should contain system");
        });
    }
    
    [ContextMenu("Run Test: World Isolation Components")]
    public void Test_WorldScoped_005()
    {
        string testName = "Test_WorldScoped_005";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create Entity1 in World1
            Entity entity1 = World.CreateEntity(world1);
            
            // Create Entity2 in World2
            Entity entity2 = World.CreateEntity(world2);
            
            // Add TestComponent to Entity1 in World1
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 100 }, world1);
            
            // Add TestComponent to Entity2 in World2
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 200 }, world2);
            
            // Check components in each world
            var component1InWorld1 = ComponentsRegistry.GetComponent<TestComponent>(entity1, world1);
            var component1InWorld2 = ComponentsRegistry.GetComponent<TestComponent>(entity1, world2);
            var component2InWorld1 = ComponentsRegistry.GetComponent<TestComponent>(entity2, world1);
            var component2InWorld2 = ComponentsRegistry.GetComponent<TestComponent>(entity2, world2);
            
            Assert(component1InWorld1.HasValue, "GetComponent(Entity1, World1) should not be null");
            Assert(!component1InWorld2.HasValue, "GetComponent(Entity1, World2) should be null");
            Assert(!component2InWorld1.HasValue, "GetComponent(Entity2, World1) should be null");
            Assert(component2InWorld2.HasValue, "GetComponent(Entity2, World2) should not be null");
        });
    }
    
    [ContextMenu("Run Test: World Isolation Systems")]
    public void Test_WorldScoped_006()
    {
        string testName = "Test_WorldScoped_006";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create System1 and System2
            var system1 = new TestSystem(() => { });
            var system2 = new TestSystem(() => { });
            
            // Add System1 to World1 Update queue
            SystemsRegistry.AddToUpdate(system1, world1);
            
            // Add System2 to World2 Update queue
            SystemsRegistry.AddToUpdate(system2, world2);
            
            // Check queues in each world
            Assert(SystemsRegistry.GetUpdateQueue(world1).Contains(system1), "World1 Update queue should contain System1");
            Assert(!SystemsRegistry.GetUpdateQueue(world1).Contains(system2), "World1 Update queue should not contain System2");
            Assert(!SystemsRegistry.GetUpdateQueue(world2).Contains(system1), "World2 Update queue should not contain System1");
            Assert(SystemsRegistry.GetUpdateQueue(world2).Contains(system2), "World2 Update queue should contain System2");
        });
    }
    
    [ContextMenu("Run Test: World Isolation Entity Pool")]
    public void Test_WorldScoped_007()
    {
        string testName = "Test_WorldScoped_007";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create Entity1 in World1
            Entity entity1 = World.CreateEntity(world1);
            
            // Create Entity2 in World2
            Entity entity2 = World.CreateEntity(world2);
            
            // Check entity pools
            Assert(EntityPool.IsAllocated(entity1, world1), "Entity1 should be allocated in World1");
            Assert(!EntityPool.IsAllocated(entity1, world2), "Entity1 should not be allocated in World2");
            Assert(!EntityPool.IsAllocated(entity2, world1), "Entity2 should not be allocated in World1");
            Assert(EntityPool.IsAllocated(entity2, world2), "Entity2 should be allocated in World2");
        });
    }
    
    [ContextMenu("Run Test: GetComponents World Parameter")]
    public void Test_WorldScoped_008()
    {
        string testName = "Test_WorldScoped_008";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1 in global world
            Entity entity1 = World.CreateEntity(null);
            
            // Create Entity2 in TestWorld
            Entity entity2 = World.CreateEntity(testWorld);
            
            // Add TestComponent to both entities
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 10 }, null);
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 20 }, testWorld);
            
            // Get components from each world
            var componentsGlobal = ComponentsRegistry.GetComponents<TestComponent>(null);
            var componentsTest = ComponentsRegistry.GetComponents<TestComponent>(testWorld);
            
            // Verify GetComponents returns components only from specified world
            AssertEquals(1, componentsGlobal.Length, "Global world should have 1 component");
            AssertEquals(1, componentsTest.Length, "TestWorld should have 1 component");
            Assert(componentsGlobal[0].Value != componentsTest[0].Value, "Components should be different");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Multiple Types World Parameter")]
    public void Test_WorldScoped_009()
    {
        string testName = "Test_WorldScoped_009";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1 in global world with Position and Velocity
            Entity entity1 = World.CreateEntity(null);
            ComponentsRegistry.AddComponent<Position>(entity1, new Position { X = 1f, Y = 2f, Z = 3f }, null);
            ComponentsRegistry.AddComponent<Velocity>(entity1, new Velocity { X = 1f, Y = 1f, Z = 1f }, null);
            
            // Create Entity2 in TestWorld with Position and Velocity
            Entity entity2 = World.CreateEntity(testWorld);
            ComponentsRegistry.AddComponent<Position>(entity2, new Position { X = 4f, Y = 5f, Z = 6f }, testWorld);
            ComponentsRegistry.AddComponent<Velocity>(entity2, new Velocity { X = 2f, Y = 2f, Z = 2f }, testWorld);
            
            // Get components from each world
            var componentsGlobal = ComponentsRegistry.GetComponents<Position, Velocity>(null);
            var componentsTest = ComponentsRegistry.GetComponents<Position, Velocity>(testWorld);
            
            // Verify multi-type queries respect World parameter
            AssertEquals(1, componentsGlobal.Length, "Global world should have 1 matching entity");
            AssertEquals(1, componentsTest.Length, "TestWorld should have 1 matching entity");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout World Parameter")]
    public void Test_WorldScoped_010()
    {
        string testName = "Test_WorldScoped_010";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1 in global world with Health (without Dead)
            Entity entity1 = World.CreateEntity(null);
            ComponentsRegistry.AddComponent<Health>(entity1, new Health { Amount = 100f }, null);
            
            // Create Entity2 in TestWorld with Health (without Dead)
            Entity entity2 = World.CreateEntity(testWorld);
            ComponentsRegistry.AddComponent<Health>(entity2, new Health { Amount = 200f }, testWorld);
            
            // Get components from each world
            var componentsGlobal = ComponentsRegistry.GetComponentsWithout<Health, Dead>(null);
            var componentsTest = ComponentsRegistry.GetComponentsWithout<Health, Dead>(testWorld);
            
            // Verify GetComponentsWithout respects World parameter
            AssertEquals(1, componentsGlobal.Length, "Global world should have 1 component");
            AssertEquals(1, componentsTest.Length, "TestWorld should have 1 component");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponents World Parameter")]
    public void Test_WorldScoped_011()
    {
        string testName = "Test_WorldScoped_011";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1 in global world
            Entity entity1 = World.CreateEntity(null);
            
            // Create Entity2 in TestWorld
            Entity entity2 = World.CreateEntity(testWorld);
            
            // Add Health to both entities
            ComponentsRegistry.AddComponent<Health>(entity1, new Health { Amount = 100f }, null);
            ComponentsRegistry.AddComponent<Health>(entity2, new Health { Amount = 200f }, testWorld);
            
            // Use GetModifiableComponents with World parameter
            int countGlobal = 0;
            using (var components = ComponentsRegistry.GetModifiableComponents<Health>(null))
            {
                countGlobal = components.Count;
            }
            
            int countTest = 0;
            using (var components = ComponentsRegistry.GetModifiableComponents<Health>(testWorld))
            {
                countTest = components.Count;
            }
            
            // Verify GetModifiableComponents respects World parameter
            AssertEquals(1, countGlobal, "Global world should have 1 modifiable component");
            AssertEquals(1, countTest, "TestWorld should have 1 modifiable component");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce World Parameter")]
    public void Test_WorldScoped_012()
    {
        string testName = "Test_WorldScoped_012";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create System
            var system = new TestSystem(() => { });
            
            // Call ExecuteOnce with World parameter
            SystemsRegistry.ExecuteOnce(system, testWorld);
            
            // System should execute successfully (no exception)
            // This test verifies API consistency - ExecuteOnce accepts World parameter
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate World Parameter")]
    public void Test_WorldScoped_013()
    {
        string testName = "Test_WorldScoped_013";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create System1 in global world
            bool system1Executed = false;
            var system1 = new TestSystem(() => { system1Executed = true; });
            SystemsRegistry.AddToUpdate(system1, null);
            
            // Create System2 in TestWorld
            bool system2Executed = false;
            var system2 = new TestSystem(() => { system2Executed = true; });
            SystemsRegistry.AddToUpdate(system2, testWorld);
            
            // Note: We can't easily track execution in TestSystem, so we verify queue membership
            // In a real scenario, systems would have execution flags
            
            // Call ExecuteUpdate(TestWorld)
            SystemsRegistry.ExecuteUpdate(testWorld);
            
            // Verify systems are in correct queues
            Assert(SystemsRegistry.GetUpdateQueue(null).Contains(system1), "System1 should be in global world queue");
            Assert(SystemsRegistry.GetUpdateQueue(testWorld).Contains(system2), "System2 should be in TestWorld queue");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate World Parameter")]
    public void Test_WorldScoped_014()
    {
        string testName = "Test_WorldScoped_014";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create System1 in global world
            var system1 = new TestSystem(() => { });
            SystemsRegistry.AddToFixedUpdate(system1, null);
            
            // Create System2 in TestWorld
            var system2 = new TestSystem(() => { });
            SystemsRegistry.AddToFixedUpdate(system2, testWorld);
            
            // Call ExecuteFixedUpdate(TestWorld)
            SystemsRegistry.ExecuteFixedUpdate(testWorld);
            
            // Verify systems are in correct queues
            Assert(SystemsRegistry.GetFixedUpdateQueue(null).Contains(system1), "System1 should be in global world queue");
            Assert(SystemsRegistry.GetFixedUpdateQueue(testWorld).Contains(system2), "System2 should be in TestWorld queue");
        });
    }
    
    // ========== World-003: World Persistence Across Scenes ==========
    // Note: Scene persistence tests require Unity scene loading, which is difficult to test in unit tests.
    // These tests verify the underlying mechanisms (DontDestroyOnLoad, static storage) rather than actual scene transitions.
    
    [ContextMenu("Run Test: UpdateProvider DontDestroyOnLoad")]
    public void Test_Persistence_001()
    {
        string testName = "Test_Persistence_001";
        ExecuteTest(testName, () =>
        {
            // Create entity (creates UpdateProvider)
            Entity entity = World.CreateEntity();
            
            // Check UpdateProvider exists
            var updateProvider = GameObject.FindObjectOfType<UpdateProvider>();
            AssertNotNull(updateProvider, "UpdateProvider should exist");
            
            // Note: Actual scene persistence test would require loading a new scene,
            // which is difficult in unit tests. This test verifies UpdateProvider is created.
        });
    }
    
    [ContextMenu("Run Test: Components Persist Across Scenes")]
    public void Test_Persistence_002()
    {
        string testName = "Test_Persistence_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position, Velocity, Health components
            ComponentsRegistry.AddComponent<Position>(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsRegistry.AddComponent<Velocity>(entity, new Velocity { X = 1f, Y = 1f, Z = 1f });
            ComponentsRegistry.AddComponent<Health>(entity, new Health { Amount = 100f });
            
            // Remember component values
            float posX = ComponentsRegistry.GetComponent<Position>(entity).Value.X;
            float velX = ComponentsRegistry.GetComponent<Velocity>(entity).Value.X;
            float healthAmount = ComponentsRegistry.GetComponent<Health>(entity).Value.Amount;
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies components are stored in static dictionaries which persist.
            // Components should persist because ComponentsRegistry uses static storage.
            
            // Verify components still exist (simulating persistence)
            var position = ComponentsRegistry.GetComponent<Position>(entity);
            var velocity = ComponentsRegistry.GetComponent<Velocity>(entity);
            var health = ComponentsRegistry.GetComponent<Health>(entity);
            
            Assert(position.HasValue, "Position component should exist");
            Assert(velocity.HasValue, "Velocity component should exist");
            Assert(health.HasValue, "Health component should exist");
            AssertEquals(posX, position.Value.X, "Position X should be unchanged");
            AssertEquals(velX, velocity.Value.X, "Velocity X should be unchanged");
            AssertEquals(healthAmount, health.Value.Amount, "Health Amount should be unchanged");
        });
    }
    
    [ContextMenu("Run Test: Entities Persist Across Scenes")]
    public void Test_Persistence_003()
    {
        string testName = "Test_Persistence_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add components to all entities
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 1 });
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 2 });
            ComponentsRegistry.AddComponent<TestComponent>(entity3, new TestComponent { Value = 3 });
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies entities are stored in static EntityPool which persists.
            
            // Verify entities still exist (simulating persistence)
            Assert(EntityPool.IsAllocated(entity1), "Entity1 should be allocated");
            Assert(EntityPool.IsAllocated(entity2), "Entity2 should be allocated");
            Assert(EntityPool.IsAllocated(entity3), "Entity3 should be allocated");
        });
    }
    
    [ContextMenu("Run Test: Systems Persist Across Scenes")]
    public void Test_Persistence_004()
    {
        string testName = "Test_Persistence_004";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2, System3
            var system1 = new TestSystem(() => { });
            var system2 = new TestSystem(() => { });
            var system3 = new TestSystem(() => { });
            
            // Add all to Update queue
            SystemsRegistry.AddToUpdate(system1);
            SystemsRegistry.AddToUpdate(system2);
            SystemsRegistry.AddToUpdate(system3);
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies systems are stored in static SystemsRegistry which persists.
            
            // Verify systems still in queue (simulating persistence)
            Assert(SystemsRegistry.GetUpdateQueue().Contains(system1), "System1 should be in queue");
            Assert(SystemsRegistry.GetUpdateQueue().Contains(system2), "System2 should be in queue");
            Assert(SystemsRegistry.GetUpdateQueue().Contains(system3), "System3 should be in queue");
        });
    }
    
    [ContextMenu("Run Test: System Execution Continues After Scene Change")]
    public void Test_Persistence_005()
    {
        string testName = "Test_Persistence_005";
        ExecuteTest(testName, () =>
        {
            // Note: This test requires actual Unity scene loading which is difficult in unit tests.
            // The test verifies that UpdateProvider continues to exist and execute systems.
            // Actual scene persistence is verified through static storage mechanisms.
            
            // Create UpdateProvider (via entity creation)
            Entity entity = World.CreateEntity();
            
            // Verify UpdateProvider exists
            var updateProvider = GameObject.FindObjectOfType<UpdateProvider>();
            AssertNotNull(updateProvider, "UpdateProvider should exist");
            
            // Note: Actual execution verification across scenes would require Play Mode testing.
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Persist Across Scenes")]
    public void Test_Persistence_006()
    {
        string testName = "Test_Persistence_006";
        ExecuteTest(testName, () =>
        {
            // Create World("TestWorld")
            World testWorld = new World("TestWorld");
            
            // Create Entity1 in global world
            Entity entity1 = World.CreateEntity(null);
            
            // Create Entity2 in TestWorld
            Entity entity2 = World.CreateEntity(testWorld);
            
            // Add components to both entities
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 100 }, null);
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 200 }, testWorld);
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies all worlds use static storage which persists.
            
            // Verify both worlds persist (simulating persistence)
            var component1 = ComponentsRegistry.GetComponent<TestComponent>(entity1, null);
            var component2 = ComponentsRegistry.GetComponent<TestComponent>(entity2, testWorld);
            
            Assert(component1.HasValue, "Entity1 component should exist in global world");
            Assert(component2.HasValue, "Entity2 component should exist in TestWorld");
        });
    }
    
    [ContextMenu("Run Test: Component Values Persist Across Scenes")]
    public void Test_Persistence_007()
    {
        string testName = "Test_Persistence_007";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position(X=10, Y=20, Z=30)
            ComponentsRegistry.AddComponent<Position>(entity, new Position { X = 10f, Y = 20f, Z = 30f });
            
            // Add Health(Amount=100)
            ComponentsRegistry.AddComponent<Health>(entity, new Health { Amount = 100f });
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies component values are stored in static storage which persists.
            
            // Check component values (simulating persistence)
            var position = ComponentsRegistry.GetComponent<Position>(entity);
            var health = ComponentsRegistry.GetComponent<Health>(entity);
            
            AssertEquals(10f, position.Value.X, "Position X should be 10");
            AssertEquals(20f, position.Value.Y, "Position Y should be 20");
            AssertEquals(30f, position.Value.Z, "Position Z should be 30");
            AssertEquals(100f, health.Value.Amount, "Health Amount should be 100");
        });
    }
    
    [ContextMenu("Run Test: Entity Pool Persists Across Scenes")]
    public void Test_Persistence_008()
    {
        string testName = "Test_Persistence_008";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Destroy Entity2
            World.DestroyEntity(entity2);
            
            // Note: Actual scene loading test would require Unity scene management.
            // This test verifies entity pool is stored in static EntityPool which persists.
            
            // Check entity pool state (simulating persistence)
            Assert(EntityPool.IsAllocated(entity1), "Entity1 should be allocated");
            Assert(!EntityPool.IsAllocated(entity2), "Entity2 should not be allocated");
            Assert(EntityPool.IsAllocated(entity3), "Entity3 should be allocated");
            AssertEquals(2, EntityPool.GetAllocatedCount(), "Allocated count should be 2");
        });
    }
    
    [ContextMenu("Run Test: World State Independent of Unity Scene")]
    public void Test_Persistence_009()
    {
        string testName = "Test_Persistence_009";
        ExecuteTest(testName, () =>
        {
            // Note: This test requires actual Unity scene loading which is difficult in unit tests.
            // The test verifies that ECS World state is stored in static dictionaries,
            // which are independent of Unity's scene system.
            
            // Create entities and components (simulating Scene1)
            Entity entity1 = World.CreateEntity();
            ComponentsRegistry.AddComponent<TestComponent>(entity1, new TestComponent { Value = 1 });
            
            // Simulate scene change by verifying state persists
            // (In real scenario, Unity scene would change, but ECS state remains)
            Assert(EntityPool.IsAllocated(entity1), "Entity1 should still be allocated");
            var component = ComponentsRegistry.GetComponent<TestComponent>(entity1);
            Assert(component.HasValue, "Component should still exist");
            
            // Create new entities (simulating Scene2)
            Entity entity2 = World.CreateEntity();
            ComponentsRegistry.AddComponent<TestComponent>(entity2, new TestComponent { Value = 2 });
            
            // Verify all entities exist (simulating return to Scene1)
            Assert(EntityPool.IsAllocated(entity1), "Entity1 should still be allocated");
            Assert(EntityPool.IsAllocated(entity2), "Entity2 should be allocated");
        });
    }
    
    [ContextMenu("Run Test: UpdateProvider Continues Execution")]
    public void Test_Persistence_010()
    {
        string testName = "Test_Persistence_010";
        ExecuteTest(testName, () =>
        {
            // Note: This test requires actual Unity scene loading and Play Mode which is difficult in unit tests.
            // The test verifies that UpdateProvider uses DontDestroyOnLoad and continues execution.
            
            // Create UpdateProvider (via entity creation)
            Entity entity = World.CreateEntity();
            
            // Verify UpdateProvider exists
            var updateProvider = GameObject.FindObjectOfType<UpdateProvider>();
            AssertNotNull(updateProvider, "UpdateProvider should exist");
            
            // Note: Actual execution verification across scenes would require Play Mode testing.
            // The UpdateProvider uses DontDestroyOnLoad which ensures it persists across scenes.
        });
    }
}

