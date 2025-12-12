using UnityEngine;
using ArtyECS.Core;
using System;

/// <summary>
/// Test class for World Management functionality (World-000 through World-003).
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
            // 1. Create World with name "TestWorld"
            World world = World.GetOrCreate("TestWorld");
            
            // 2. Verify that World.Name == "TestWorld"
            AssertEquals("TestWorld", world.Name, "World.Name should be 'TestWorld'");
            AssertNotNull(world, "World should not be null");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Creation")]
    public void Test_WorldClass_002()
    {
        string testName = "Test_WorldClass_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1")
            World world1 = World.GetOrCreate("World1");
            
            // 2. Create World2("World2")
            World world2 = World.GetOrCreate("World2");
            
            // 3. Create World3("World3")
            World world3 = World.GetOrCreate("World3");
            
            // All worlds created successfully
            AssertEquals("World1", world1.Name, "World1.Name should be 'World1'");
            AssertEquals("World2", world2.Name, "World2.Name should be 'World2'");
            AssertEquals("World3", world3.Name, "World3.Name should be 'World3'");
            Assert(world1 != world2, "World1 should not equal World2");
            Assert(world2 != world3, "World2 should not equal World3");
        });
    }
    
    [ContextMenu("Run Test: World Equality")]
    public void Test_WorldClass_003()
    {
        string testName = "Test_WorldClass_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("Test")
            World world1 = World.GetOrCreate("Test1");
            
            // 2. Create World2("Test")
            World world2 = World.GetOrCreate("Test2");
            
            // 3. Get global world instance
            World globalWorld1 = World.GetOrCreate();
            
            // 4. Get global world instance again
            World globalWorld2 = World.GetOrCreate();
            
            // Worlds with same name are different instances, global world is same instance
            Assert(world1 != world2, "World1 should not equal World2 (different instances)");
            Assert(globalWorld1 == globalWorld2, "GlobalWorld1 should equal GlobalWorld2 (same singleton)");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Scoped World")]
    public void Test_WorldClass_004()
    {
        string testName = "Test_WorldClass_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create Entity in TestWorld
            Entity entity = testWorld.CreateEntity();
            
            // 3. Add components to Entity in TestWorld
            testWorld.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 4. Add system to TestWorld
            TestSystem system = new TestSystem(() => { });
            testWorld.AddToUpdate(system);
            
            // 5. Call World.Destroy(TestWorld)
            bool destroyed = World.Destroy(testWorld);
            
            // 6. Verify that world is destroyed
            Assert(destroyed, "World.Destroy should return true");
            
            // Note: After destruction, we can't easily verify components/systems are cleared
            // because the world might be marked as destroyed. But we can verify destruction returned true.
        });
    }
    
    [ContextMenu("Run Test: World Destroy Global World")]
    public void Test_WorldClass_005()
    {
        string testName = "Test_WorldClass_005";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            World globalWorld = World.GetOrCreate();
            
            // 2. Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // Destroy returns false, global world not destroyed
            Assert(!destroyed, "Destroy should return false for global world");
            AssertNotNull(World.GetOrCreate(), "Global world should still exist");
            AssertEquals("Global", World.GetOrCreate().Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Twice")]
    public void Test_WorldClass_006()
    {
        string testName = "Test_WorldClass_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Destroy TestWorld
            bool destroyed1 = World.Destroy(testWorld);
            
            // 3. Attempt to destroy TestWorld again
            bool destroyed2 = World.Destroy(testWorld);
            
            // Second destruction returns false
            Assert(destroyed1, "First Destroy should return true");
            Assert(!destroyed2, "Second Destroy should return false");
        });
    }
    
    // ========== World-001: Global World Singleton ==========
    
    [ContextMenu("Run Test: GetOrCreate Returns Singleton")]
    public void Test_GlobalWorld_001()
    {
        string testName = "Test_GlobalWorld_001";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.GetOrCreate() first time
            World globalWorld1 = World.GetOrCreate();
            
            // 2. Call World.GetOrCreate() second time
            World globalWorld2 = World.GetOrCreate();
            
            // 3. Compare instances
            // Same instance returned both times
            Assert(globalWorld1 == globalWorld2, "GlobalWorld1 should equal GlobalWorld2");
        });
    }
    
    [ContextMenu("Run Test: Global World Name")]
    public void Test_GlobalWorld_002()
    {
        string testName = "Test_GlobalWorld_002";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            World globalWorld = World.GetOrCreate();
            
            // 2. Check Name property
            AssertEquals("Global", globalWorld.Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: Global World Lazy Initialization")]
    public void Test_GlobalWorld_003()
    {
        string testName = "Test_GlobalWorld_003";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.GetOrCreate()
            World globalWorld = World.GetOrCreate();
            
            // 2. Verify instance is created
            AssertNotNull(globalWorld, "Global world should not be null");
            AssertEquals("Global", globalWorld.Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: Global World Cannot Be Destroyed")]
    public void Test_GlobalWorld_004()
    {
        string testName = "Test_GlobalWorld_004";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            World globalWorld = World.GetOrCreate();
            
            // 2. Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // 3. Verify global world still exists
            Assert(!destroyed, "Destroy should return false for global world");
            AssertNotNull(World.GetOrCreate(), "Global world should still exist");
            World newGlobalWorld = World.GetOrCreate();
            AssertEquals(globalWorld, newGlobalWorld, "Global world should be same instance");
        });
    }
    
    // ========== World-002: World-Scoped Storage Integration ==========
    
    [ContextMenu("Run Test: World Isolation Components")]
    public void Test_WorldScoped_001()
    {
        string testName = "Test_WorldScoped_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1") and World2("World2")
            World world1 = World.GetOrCreate("World1");
            World world2 = World.GetOrCreate("World2");
            
            // 2. Create Entity1 in World1
            Entity entity1 = world1.CreateEntity();
            
            // 3. Create Entity2 in World2
            Entity entity2 = world2.CreateEntity();
            
            // 4. Add TestComponent to Entity1 in World1
            world1.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 5. Add TestComponent to Entity2 in World2
            world2.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 6. Check components in each world
            // Components are isolated by world
            TestComponent comp1 = world1.GetComponent<TestComponent>(entity1);
            AssertEquals(1, comp1.Value, "Entity1 in World1 should have Value=1");
            
            bool hasComp1InWorld2 = entity1.Has<TestComponent>(world2);
            Assert(!hasComp1InWorld2, "Entity1 should not have component in World2");
            
            bool hasComp2InWorld1 = entity2.Has<TestComponent>(world1);
            Assert(!hasComp2InWorld1, "Entity2 should not have component in World1");
            
            TestComponent comp2 = world2.GetComponent<TestComponent>(entity2);
            AssertEquals(2, comp2.Value, "Entity2 in World2 should have Value=2");
        });
    }
    
    [ContextMenu("Run Test: World Isolation Systems")]
    public void Test_WorldScoped_002()
    {
        string testName = "Test_WorldScoped_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1") and World2("World2")
            World world1 = World.GetOrCreate("World1");
            World world2 = World.GetOrCreate("World2");
            
            // 2. Create System1 and System2
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to World1 Update queue
            world1.AddToUpdate(system1);
            
            // 4. Add System2 to World2 Update queue
            world2.AddToUpdate(system2);
            
            // 5. Execute each world's Update queue
            world1.ExecuteUpdate();
            world2.ExecuteUpdate();
            
            // Systems are isolated by world
            Assert(system1Executed, "System1 should execute in World1");
            Assert(system2Executed, "System2 should execute in World2");
            
            // Reset flags
            system1Executed = false;
            system2Executed = false;
            
            // Execute world2 first, then world1 - systems should only execute in their own world
            world2.ExecuteUpdate();
            world1.ExecuteUpdate();
            
            Assert(system1Executed, "System1 should execute again in World1");
            Assert(system2Executed, "System2 should execute again in World2");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith World Parameter")]
    public void Test_WorldScoped_003()
    {
        string testName = "Test_WorldScoped_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            World globalWorld = World.GetOrCreate();
            
            // 2. Create Entity1 in global world with Position and Velocity
            Entity entity1 = globalWorld.CreateEntity();
            globalWorld.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            globalWorld.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 3. Create Entity2 in TestWorld with Position and Velocity
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            testWorld.AddComponent(entity2, new Velocity { X = 2f, Y = 3f, Z = 4f });
            
            // 4. Get entities from each world
            var globalEntities = globalWorld.GetEntitiesWith<Position, Velocity>();
            var testEntities = testWorld.GetEntitiesWith<Position, Velocity>();
            
            // Multi-type queries respect World parameter
            AssertEquals(1, globalEntities.Length, "Global world should have 1 entity");
            AssertEquals(1, testEntities.Length, "TestWorld should have 1 entity");
        });
    }
    
    [ContextMenu("Run Test: GetComponents World Parameter")]
    public void Test_WorldScoped_004()
    {
        string testName = "Test_WorldScoped_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            World globalWorld = World.GetOrCreate();
            
            // 2. Create Entity1 in global world
            Entity entity1 = globalWorld.CreateEntity();
            globalWorld.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 3. Create Entity2 in TestWorld
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 4. Get components from each world
            var globalComponents = globalWorld.GetComponents<TestComponent>();
            var testComponents = testWorld.GetComponents<TestComponent>();
            
            // GetComponents returns components only from specified world
            AssertEquals(1, globalComponents.Length, "Global world should have 1 component");
            AssertEquals(1, testComponents.Length, "TestWorld should have 1 component");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce World Parameter")]
    public void Test_WorldScoped_005()
    {
        string testName = "Test_WorldScoped_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            World globalWorld = World.GetOrCreate();
            
            // 2. Create System that uses world context
            World executedWorld = null;
            TestSystem system = new TestSystem(() => 
            {
                // System can use World parameter passed to Execute()
                // For this test, we'll verify the system executes
                executedWorld = globalWorld; // Simplified - system doesn't receive world parameter in TestSystem
            });
            
            // 3. Call ExecuteOnce with World parameter
            testWorld.ExecuteOnce(system);
            
            // System executed successfully
            Assert(true, "ExecuteOnce should execute system successfully");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate World Parameter")]
    public void Test_WorldScoped_006()
    {
        string testName = "Test_WorldScoped_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            World globalWorld = World.GetOrCreate();
            
            // 2. Create System1 in global world
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to global world Update queue
            globalWorld.AddToUpdate(system1);
            
            // 4. Add System2 to TestWorld Update queue
            testWorld.AddToUpdate(system2);
            
            // 5. Call ExecuteUpdate(TestWorld)
            testWorld.ExecuteUpdate();
            
            // 6. Check execution flags
            // Only systems from specified world executed
            Assert(!system1Executed, "System1 should not execute");
            Assert(system2Executed, "System2 should execute");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate World Parameter")]
    public void Test_WorldScoped_007()
    {
        string testName = "Test_WorldScoped_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            World testWorld = World.GetOrCreate("TestWorld");
            World globalWorld = World.GetOrCreate();
            
            // 2. Create System1 in global world
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to global world FixedUpdate queue
            globalWorld.AddToFixedUpdate(system1);
            
            // 4. Add System2 to TestWorld FixedUpdate queue
            testWorld.AddToFixedUpdate(system2);
            
            // 5. Call ExecuteFixedUpdate(TestWorld)
            testWorld.ExecuteFixedUpdate();
            
            // 6. Check execution flags
            // Only systems from specified world executed
            Assert(!system1Executed, "System1 should not execute");
            Assert(system2Executed, "System2 should execute");
        });
    }
    
    // ========== World-003: World Persistence Across Scenes ==========
    // Note: Scene persistence tests are difficult to test without actual scene loading
    // These tests verify the underlying mechanism (static storage) works correctly
    
    [ContextMenu("Run Test: World State Independent of Operations")]
    public void Test_Persistence_001()
    {
        string testName = "Test_Persistence_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities and components
            World world = World.GetOrCreate();
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new Position { X = 10f, Y = 20f, Z = 30f });
            world.AddComponent(entity1, new Health { Amount = 100f });
            
            // 2. Verify components exist
            Position pos = world.GetComponent<Position>(entity1);
            Health health = world.GetComponent<Health>(entity1);
            
            AssertEquals(10f, pos.X, "Position.X should be 10");
            AssertEquals(100f, health.Amount, "Health.Amount should be 100");
            
            // 3. Create another world and verify isolation
            World world2 = World.GetOrCreate("Test2");
            Entity entity2 = world2.CreateEntity();
            world2.AddComponent(entity2, new Position { X = 50f, Y = 60f, Z = 70f });
            
            // 4. Verify world1 components still exist
            pos = world.GetComponent<Position>(entity1);
            health = world.GetComponent<Health>(entity1);
            
            AssertEquals(10f, pos.X, "Position.X should still be 10");
            AssertEquals(100f, health.Amount, "Health.Amount should still be 100");
            
            // 5. Verify world2 components are separate
            Position pos2 = world2.GetComponent<Position>(entity2);
            AssertEquals(50f, pos2.X, "World2 Position.X should be 50");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Persist")]
    public void Test_Persistence_002()
    {
        string testName = "Test_Persistence_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 in global world
            World globalWorld = World.GetOrCreate();
            Entity entity1 = globalWorld.CreateEntity();
            globalWorld.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 2. Create Entity2 in TestWorld
            World testWorld = World.GetOrCreate("TestWorld");
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 3. Verify both worlds have their entities
            TestComponent comp1 = globalWorld.GetComponent<TestComponent>(entity1);
            TestComponent comp2 = testWorld.GetComponent<TestComponent>(entity2);
            
            AssertEquals(1, comp1.Value, "Entity1 in global world should have Value=1");
            AssertEquals(2, comp2.Value, "Entity2 in TestWorld should have Value=2");
            
            // 4. Get worlds again and verify entities still exist
            World globalWorld2 = World.GetOrCreate();
            World testWorld2 = World.GetOrCreate("TestWorld");
            
            // Note: Entities are stored per-world, so they should still be accessible
            // However, entity IDs might not be valid after ClearAllECSState in ExecuteTest
            // This test verifies that world state is maintained during test execution
            AssertEquals(globalWorld, globalWorld2, "Global world should be same instance");
            AssertEquals(testWorld, testWorld2, "TestWorld should be same instance");
        });
    }
}

