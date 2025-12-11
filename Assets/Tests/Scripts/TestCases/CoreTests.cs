using UnityEngine;
using ArtyECS.Core;
using System;
using System.Linq;

/// <summary>
/// Test cases for ArtyECS Core functionality (Entity, Component, ComponentsManager, etc.)
/// </summary>
public class CoreTests : TestBase
{
    // ========== Core-000: Entity Implementation ==========
    
    [ContextMenu("Run Test: Entity Creation with ID and Generation")]
    public void Test_Entity_001()
    {
        string testName = "Test_Entity_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity with id=5, generation=2
            Entity entity = new Entity(5, 2);
            
            // Verify that Entity.Id == 5
            AssertEquals(5, entity.Id, "Entity.Id should be 5");
            
            // Verify that Entity.Generation == 2
            AssertEquals(2, entity.Generation, "Entity.Generation should be 2");
            
            // Verify that Entity.IsValid == true
            Assert(entity.IsValid, "Entity.IsValid should be true");
        });
    }
    
    [ContextMenu("Run Test: Invalid Entity")]
    public void Test_Entity_002()
    {
        string testName = "Test_Entity_002";
        ExecuteTest(testName, () =>
        {
            // Get Entity.Invalid
            Entity invalid = Entity.Invalid;
            
            // Check IsValid
            AssertEquals(-1, invalid.Id, "Entity.Invalid.Id should be -1");
            AssertEquals(0, invalid.Generation, "Entity.Invalid.Generation should be 0");
            Assert(!invalid.IsValid, "Entity.Invalid.IsValid should be false");
        });
    }
    
    [ContextMenu("Run Test: Entity Equality")]
    public void Test_Entity_003()
    {
        string testName = "Test_Entity_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with id=1, generation=0
            Entity entity1 = new Entity(1, 0);
            
            // Create Entity2 with id=1, generation=0
            Entity entity2 = new Entity(1, 0);
            
            // Create Entity3 with id=1, generation=1
            Entity entity3 = new Entity(1, 1);
            
            // Create Entity4 with id=2, generation=0
            Entity entity4 = new Entity(2, 0);
            
            // Verify equality
            Assert(entity1 == entity2, "Entity1 == Entity2 should be true");
            Assert(entity1 != entity3, "Entity1 != Entity3 should be true");
            Assert(entity1 != entity4, "Entity1 != Entity4 should be true");
            Assert(entity1.Equals(entity2), "Entity1.Equals(Entity2) should be true");
            Assert(!entity1.Equals(entity3), "Entity1.Equals(Entity3) should be false");
        });
    }
    
    [ContextMenu("Run Test: Entity HashCode")]
    public void Test_Entity_004()
    {
        string testName = "Test_Entity_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with id=1, generation=0
            Entity entity1 = new Entity(1, 0);
            
            // Create Entity2 with id=1, generation=0
            Entity entity2 = new Entity(1, 0);
            
            // Create Entity3 with id=1, generation=1
            Entity entity3 = new Entity(1, 1);
            
            // Get hash codes
            int hash1 = entity1.GetHashCode();
            int hash2 = entity2.GetHashCode();
            int hash3 = entity3.GetHashCode();
            
            // Verify hash codes
            AssertEquals(hash1, hash2, "Entity1 and Entity2 should have same hash code");
            Assert(hash1 != hash3, "Entity1 and Entity3 should have different hash codes");
        });
    }
    
    [ContextMenu("Run Test: Entity ToString")]
    public void Test_Entity_005()
    {
        string testName = "Test_Entity_005";
        ExecuteTest(testName, () =>
        {
            // Create Entity with id=42, generation=3
            Entity entity = new Entity(42, 3);
            
            // Call ToString()
            string str = entity.ToString();
            
            // Verify string contains ID and Generation
            Assert(str.Contains("42"), "ToString() should contain ID 42");
            Assert(str.Contains("3"), "ToString() should contain Generation 3");
        });
    }
    
    // ========== Core-001: Component Base/Marker Interface ==========
    
    [ContextMenu("Run Test: Component Implements IComponent")]
    public void Test_Component_001()
    {
        string testName = "Test_Component_001";
        ExecuteTest(testName, () =>
        {
            // Create TestComponent instance
            TestComponent testComponent = new TestComponent { Value = 10 };
            
            // Verify that it implements IComponent
            Assert(testComponent is IComponent, "TestComponent should implement IComponent");
        });
    }
    
    [ContextMenu("Run Test: Multiple Components Implement IComponent")]
    public void Test_Component_002()
    {
        string testName = "Test_Component_002";
        ExecuteTest(testName, () =>
        {
            // Create Position : IComponent
            Position position = new Position { X = 1f, Y = 2f, Z = 3f };
            
            // Create Velocity : IComponent
            Velocity velocity = new Velocity { X = 1f, Y = 2f, Z = 3f };
            
            // Create Health : IComponent
            Health health = new Health { Amount = 100f };
            
            // Verify all components implement IComponent
            Assert(position is IComponent, "Position should implement IComponent");
            Assert(velocity is IComponent, "Velocity should implement IComponent");
            Assert(health is IComponent, "Health should implement IComponent");
        });
    }
    
    // ========== Core-002: ComponentsManager - Basic Structure ==========
    
    [ContextMenu("Run Test: GetGlobalWorld Returns World")]
    public void Test_Storage_001()
    {
        string testName = "Test_Storage_001";
        ExecuteTest(testName, () =>
        {
            // Call ComponentsManager.GetGlobalWorld()
            World world = ComponentsManager.GetGlobalWorld();
            
            // Verify world is not null
            AssertNotNull(world, "GetGlobalWorld() should return non-null World");
            
            // Verify world name is "Global"
            AssertEquals("Global", world.Name, "World.Name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: IsWorldInitialized for Global World")]
    public void Test_Storage_002()
    {
        string testName = "Test_Storage_002";
        ExecuteTest(testName, () =>
        {
            // Check IsWorldInitialized() before usage (may be false or true)
            bool beforeUsage = ComponentsManager.IsWorldInitialized();
            
            // Add component (initializes world)
            Entity entity = World.CreateEntity();
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            
            // Check IsWorldInitialized() after usage
            bool afterUsage = ComponentsManager.IsWorldInitialized();
            
            // Verify world is initialized after usage
            Assert(afterUsage, "IsWorldInitialized() should be true after usage");
        });
    }
    
    [ContextMenu("Run Test: GetWorldCount")]
    public void Test_Storage_003()
    {
        string testName = "Test_Storage_003";
        ExecuteTest(testName, () =>
        {
            // Get initial world count
            int initialCount = ComponentsManager.GetWorldCount();
            
            // Create new World("Test")
            World testWorld = new World("Test");
            
            // Add component to new world
            Entity entity = World.CreateEntity(testWorld);
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 }, testWorld);
            
            // Get new world count
            int newCount = ComponentsManager.GetWorldCount();
            
            // Verify world count increased
            Assert(initialCount >= 0, "Initial count should be >= 0");
            Assert(newCount == initialCount + 1, "New count should be initialCount + 1");
        });
    }
    
    [ContextMenu("Run Test: World Scoped Storage Isolation")]
    public void Test_Storage_004()
    {
        string testName = "Test_Storage_004";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create Entity1 and Entity2
            Entity entity1 = World.CreateEntity(world1);
            Entity entity2 = World.CreateEntity(world2);
            
            // Add component to Entity1 in World1
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 }, world1);
            
            // Add component to Entity2 in World2
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 }, world2);
            
            // Check components in each world
            var comp1World1 = ComponentsManager.GetComponent<TestComponent>(entity1, world1);
            var comp1World2 = ComponentsManager.GetComponent<TestComponent>(entity1, world2);
            var comp2World1 = ComponentsManager.GetComponent<TestComponent>(entity2, world1);
            var comp2World2 = ComponentsManager.GetComponent<TestComponent>(entity2, world2);
            
            // Verify isolation
            Assert(comp1World1.HasValue, "Entity1 should have component in World1");
            Assert(!comp1World2.HasValue, "Entity1 should not have component in World2");
            Assert(!comp2World1.HasValue, "Entity2 should not have component in World1");
            Assert(comp2World2.HasValue, "Entity2 should have component in World2");
        });
    }
    
    // ========== Core-003: ComponentsManager - Single Component Type Storage ==========
    
    [ContextMenu("Run Test: ComponentTable Initial Capacity")]
    public void Test_Storage_005()
    {
        string testName = "Test_Storage_005";
        ExecuteTest(testName, () =>
        {
            // Add first component
            Entity entity = World.CreateEntity();
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            
            // Verify that storage is created (can be verified through component addition)
            var component = ComponentsManager.GetComponent<TestComponent>(entity);
            Assert(component.HasValue, "Component should be successfully added");
            AssertEquals(1, component.Value.Value, "Component value should be 1");
        });
    }
    
    [ContextMenu("Run Test: ComponentTable Capacity Growth")]
    public void Test_Storage_006()
    {
        string testName = "Test_Storage_006";
        ExecuteTest(testName, () =>
        {
            // Add 20 components (more than default capacity 16)
            Entity[] entities = new Entity[20];
            for (int i = 0; i < 20; i++)
            {
                entities[i] = World.CreateEntity();
                ComponentsManager.AddComponent(entities[i], new TestComponent { Value = i });
            }
            
            // Verify that all components are added
            var components = ComponentsManager.GetComponents<TestComponent>();
            AssertEquals(20, components.Length, "All 20 components should be added");
        });
    }
    
    [ContextMenu("Run Test: ComponentTable Contiguous Memory Layout")]
    public void Test_Storage_007()
    {
        string testName = "Test_Storage_007";
        ExecuteTest(testName, () =>
        {
            // Add 5 components
            Entity[] entities = new Entity[5];
            for (int i = 0; i < 5; i++)
            {
                entities[i] = World.CreateEntity();
                ComponentsManager.AddComponent(entities[i], new TestComponent { Value = i });
            }
            
            // Get ReadOnlySpan of components
            var span = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify that all components are accessible
            AssertEquals(5, span.Length, "Span should contain 5 components");
            for (int i = 0; i < 5; i++)
            {
                AssertEquals(i, span[i].Value, $"Component {i} should have value {i}");
            }
        });
    }
    
    // ========== Core-004: ComponentsManager - Add Component ==========
    
    [ContextMenu("Run Test: Add Single Component")]
    public void Test_Add_001()
    {
        string testName = "Test_Add_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent with value
            TestComponent component = new TestComponent { Value = 42 };
            ComponentsManager.AddComponent(entity, component);
            
            // Get component back
            var retrieved = ComponentsManager.GetComponent<TestComponent>(entity);
            
            // Verify component was added
            Assert(retrieved.HasValue, "Component should exist");
            AssertEquals(42, retrieved.Value.Value, "Component value should be 42");
        });
    }
    
    [ContextMenu("Run Test: Add Multiple Different Components")]
    public void Test_Add_002()
    {
        string testName = "Test_Add_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position component
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            
            // Add Velocity component
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            // Add Health component
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Get all components
            var position = ComponentsManager.GetComponent<Position>(entity);
            var velocity = ComponentsManager.GetComponent<Velocity>(entity);
            var health = ComponentsManager.GetComponent<Health>(entity);
            
            // Verify all components added
            Assert(position.HasValue, "Position component should exist");
            Assert(velocity.HasValue, "Velocity component should exist");
            Assert(health.HasValue, "Health component should exist");
        });
    }
    
    [ContextMenu("Run Test: Add Duplicate Component Throws Exception")]
    public void Test_Add_003()
    {
        string testName = "Test_Add_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            
            // Attempt to add TestComponent again
            bool exceptionThrown = false;
            try
            {
                ComponentsManager.AddComponent(entity, new TestComponent { Value = 2 });
            }
            catch (InvalidOperationException ex)
            {
                exceptionThrown = true;
                Assert(ex.Message.Contains("already has") || ex.Message.Contains("duplicate"), 
                    "Exception message should contain information about duplicate");
            }
            
            Assert(exceptionThrown, "InvalidOperationException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: Add Component to Multiple Entities")]
    public void Test_Add_004()
    {
        string testName = "Test_Add_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add TestComponent to each with different values
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 });
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 });
            ComponentsManager.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // Get components back
            var comp1 = ComponentsManager.GetComponent<TestComponent>(entity1);
            var comp2 = ComponentsManager.GetComponent<TestComponent>(entity2);
            var comp3 = ComponentsManager.GetComponent<TestComponent>(entity3);
            
            // Verify each entity has its own component
            AssertEquals(1, comp1.Value.Value, "Entity1 should have value 1");
            AssertEquals(2, comp2.Value.Value, "Entity2 should have value 2");
            AssertEquals(3, comp3.Value.Value, "Entity3 should have value 3");
        });
    }
    
    [ContextMenu("Run Test: Add Component with World Parameter")]
    public void Test_Add_005()
    {
        string testName = "Test_Add_005";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity
            Entity entity = World.CreateEntity(testWorld);
            
            // Add component to specified world
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 42 }, testWorld);
            
            // Get component from specified world
            var compInWorld = ComponentsManager.GetComponent<TestComponent>(entity, testWorld);
            
            // Attempt to get component from global world
            var compGlobal = ComponentsManager.GetComponent<TestComponent>(entity, null);
            
            // Verify component accessible only in specified world
            Assert(compInWorld.HasValue, "Component should exist in testWorld");
            Assert(!compGlobal.HasValue, "Component should not exist in global world");
        });
    }
    
    // ========== Core-005: ComponentsManager - Remove Component ==========
    
    [ContextMenu("Run Test: Remove Existing Component")]
    public void Test_Remove_001()
    {
        string testName = "Test_Remove_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            
            // Remove TestComponent
            bool removed = ComponentsManager.RemoveComponent<TestComponent>(entity);
            
            // Verify that component is removed
            Assert(removed, "RemoveComponent should return true");
            var component = ComponentsManager.GetComponent<TestComponent>(entity);
            Assert(!component.HasValue, "Component should not exist after removal");
        });
    }
    
    [ContextMenu("Run Test: Remove Non-Existent Component")]
    public void Test_Remove_002()
    {
        string testName = "Test_Remove_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity without component
            Entity entity = World.CreateEntity();
            
            // Attempt to remove non-existent component
            bool removed = ComponentsManager.RemoveComponent<TestComponent>(entity);
            
            // Verify returns false, no exception thrown
            Assert(!removed, "RemoveComponent should return false");
        });
    }
    
    [ContextMenu("Run Test: Remove Component Maintains Other Components")]
    public void Test_Remove_003()
    {
        string testName = "Test_Remove_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position, Velocity, Health
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Remove Velocity
            ComponentsManager.RemoveComponent<Velocity>(entity);
            
            // Check remaining components
            var position = ComponentsManager.GetComponent<Position>(entity);
            var velocity = ComponentsManager.GetComponent<Velocity>(entity);
            var health = ComponentsManager.GetComponent<Health>(entity);
            
            // Verify Position and Health remain, Velocity removed
            Assert(position.HasValue, "Position should still exist");
            Assert(!velocity.HasValue, "Velocity should be removed");
            Assert(health.HasValue, "Health should still exist");
        });
    }
    
    [ContextMenu("Run Test: Remove Component Swap With Last Element")]
    public void Test_Remove_004()
    {
        string testName = "Test_Remove_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add components to all three
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 });
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 });
            ComponentsManager.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // Remove component from Entity2 (middle)
            ComponentsManager.RemoveComponent<TestComponent>(entity2);
            
            // Verify that Entity1 and Entity3 still have components
            var comp1 = ComponentsManager.GetComponent<TestComponent>(entity1);
            var comp2 = ComponentsManager.GetComponent<TestComponent>(entity2);
            var comp3 = ComponentsManager.GetComponent<TestComponent>(entity3);
            
            Assert(comp1.HasValue, "Entity1 should still have component");
            Assert(!comp2.HasValue, "Entity2 should not have component");
            Assert(comp3.HasValue, "Entity3 should still have component");
        });
    }
    
    [ContextMenu("Run Test: Remove Component with World Parameter")]
    public void Test_Remove_005()
    {
        string testName = "Test_Remove_005";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity
            Entity entity = World.CreateEntity(testWorld);
            
            // Add component to testWorld and to global world
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 }, testWorld);
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 2 }, null);
            
            // Remove component from testWorld
            ComponentsManager.RemoveComponent<TestComponent>(entity, testWorld);
            
            // Check components in both worlds
            var compTest = ComponentsManager.GetComponent<TestComponent>(entity, testWorld);
            var compGlobal = ComponentsManager.GetComponent<TestComponent>(entity, null);
            
            // Verify component removed only from specified world
            Assert(!compTest.HasValue, "Component should be removed from testWorld");
            Assert(compGlobal.HasValue, "Component should remain in global world");
        });
    }
    
    // ========== Core-006: ComponentsManager - Get Component (Single Entity) ==========
    
    [ContextMenu("Run Test: Get Existing Component")]
    public void Test_Get_001()
    {
        string testName = "Test_Get_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent with known value
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 42 });
            
            // Get component
            var component = ComponentsManager.GetComponent<TestComponent>(entity);
            
            // Verify component returned with correct value
            Assert(component.HasValue, "Component should exist");
            AssertEquals(42, component.Value.Value, "Component value should be 42");
        });
    }
    
    [ContextMenu("Run Test: Get Non-Existent Component")]
    public void Test_Get_002()
    {
        string testName = "Test_Get_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity without component
            Entity entity = World.CreateEntity();
            
            // Get non-existent component
            var component = ComponentsManager.GetComponent<TestComponent>(entity);
            
            // Verify returns null (nullable struct)
            Assert(!component.HasValue, "Component should not exist");
        });
    }
    
    [ContextMenu("Run Test: Get Component After Removal")]
    public void Test_Get_003()
    {
        string testName = "Test_Get_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            
            // Remove TestComponent
            ComponentsManager.RemoveComponent<TestComponent>(entity);
            
            // Get TestComponent
            var component = ComponentsManager.GetComponent<TestComponent>(entity);
            
            // Verify returns null
            Assert(!component.HasValue, "Component should not exist after removal");
        });
    }
    
    [ContextMenu("Run Test: Get Component with World Parameter")]
    public void Test_Get_004()
    {
        string testName = "Test_Get_004";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity
            Entity entity = World.CreateEntity(testWorld);
            
            // Add component to testWorld
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 42 }, testWorld);
            
            // Get component from testWorld
            var compTest = ComponentsManager.GetComponent<TestComponent>(entity, testWorld);
            
            // Get component from global world
            var compGlobal = ComponentsManager.GetComponent<TestComponent>(entity, null);
            
            // Verify component accessible only in specified world
            Assert(compTest.HasValue, "Component should exist in testWorld");
            Assert(!compGlobal.HasValue, "Component should not exist in global world");
        });
    }
    
    // ========== Core-007: ComponentsManager - GetComponents (Single Type Query) ==========
    
    [ContextMenu("Run Test: GetComponents Empty Storage")]
    public void Test_Query_001()
    {
        string testName = "Test_Query_001";
        ExecuteTest(testName, () =>
        {
            // Get components from empty storage
            var components = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify returns empty span
            AssertEquals(0, components.Length, "Components.Length should be 0");
            Assert(components.IsEmpty, "Components should be empty");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Single Component")]
    public void Test_Query_002()
    {
        string testName = "Test_Query_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add TestComponent
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 42 });
            
            // Get all components
            var components = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify returns span with one component
            AssertEquals(1, components.Length, "Components.Length should be 1");
            AssertEquals(42, components[0].Value, "Component value should be 42");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Multiple Components")]
    public void Test_Query_003()
    {
        string testName = "Test_Query_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add TestComponent to each with different values
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 });
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 });
            ComponentsManager.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // Get all components
            var components = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify returns span with three components
            AssertEquals(3, components.Length, "Components.Length should be 3");
            
            // Verify all three components present
            var values = new int[] { components[0].Value, components[1].Value, components[2].Value };
            Assert(values.Contains(1), "Component with value 1 should be present");
            Assert(values.Contains(2), "Component with value 2 should be present");
            Assert(values.Contains(3), "Component with value 3 should be present");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Zero-Allocation")]
    public void Test_Query_004()
    {
        string testName = "Test_Query_004";
        ExecuteTest(testName, () =>
        {
            // Add several components
            Entity[] entities = new Entity[5];
            for (int i = 0; i < 5; i++)
            {
                entities[i] = World.CreateEntity();
                ComponentsManager.AddComponent(entities[i], new TestComponent { Value = i });
            }
            
            // Get components through GetComponents
            var components = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify that it's ReadOnlySpan (not array)
            // This is verified by the fact that we can iterate without allocations
            AssertEquals(5, components.Length, "Components.Length should be 5");
            
            // ReadOnlySpan<T> is a value type, so this test passes if we can use it
            // The actual zero-allocation guarantee is verified through profiling
        });
    }
    
    [ContextMenu("Run Test: GetComponents After Removal")]
    public void Test_Query_005()
    {
        string testName = "Test_Query_005";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add components to all three
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 });
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 });
            ComponentsManager.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // Remove component from Entity2
            ComponentsManager.RemoveComponent<TestComponent>(entity2);
            
            // Get all components
            var components = ComponentsManager.GetComponents<TestComponent>();
            
            // Verify returns span with two components
            AssertEquals(2, components.Length, "Components.Length should be 2");
        });
    }
    
    [ContextMenu("Run Test: GetComponents with World Parameter")]
    public void Test_Query_006()
    {
        string testName = "Test_Query_006";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity1, Entity2
            Entity entity1 = World.CreateEntity(testWorld);
            Entity entity2 = World.CreateEntity(null);
            
            // Add components to testWorld and to global world
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 }, testWorld);
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 }, null);
            
            // Get components from testWorld
            var componentsTest = ComponentsManager.GetComponents<TestComponent>(testWorld);
            
            // Get components from global world
            var componentsGlobal = ComponentsManager.GetComponents<TestComponent>(null);
            
            // Verify components are isolated by worlds
            AssertEquals(1, componentsTest.Length, "TestWorld should have 1 component");
            AssertEquals(1, componentsGlobal.Length, "Global world should have 1 component");
        });
    }
    
    // ========== Core-008: ComponentsManager - GetComponents (Multiple AND Query) ==========
    
    [ContextMenu("Run Test: GetComponents Two Types - Both Present")]
    public void Test_Query_007()
    {
        string testName = "Test_Query_007";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Position and Velocity
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity1, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            // Create Entity2 only with Position
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            
            // Create Entity3 only with Velocity
            Entity entity3 = World.CreateEntity();
            ComponentsManager.AddComponent(entity3, new Velocity { X = 40f, Y = 50f, Z = 60f });
            
            // Call GetComponents<Position, Velocity>()
            var components = ComponentsManager.GetComponents<Position, Velocity>();
            
            // Verify returns only Entity1 (has both components)
            AssertEquals(1, components.Length, "Should return 1 component");
            AssertEquals(1f, components[0].X, "Position X should be 1f");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Two Types - No Match")]
    public void Test_Query_008()
    {
        string testName = "Test_Query_008";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 only with Position
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            
            // Create Entity2 only with Velocity
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            // Call GetComponents<Position, Velocity>()
            var components = ComponentsManager.GetComponents<Position, Velocity>();
            
            // Verify returns empty span
            AssertEquals(0, components.Length, "Should return 0 components");
            Assert(components.IsEmpty, "Should be empty");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Two Types - Multiple Matches")]
    public void Test_Query_009()
    {
        string testName = "Test_Query_009";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3 with Position and Velocity
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity1, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            ComponentsManager.AddComponent(entity2, new Velocity { X = 40f, Y = 50f, Z = 60f });
            
            Entity entity3 = World.CreateEntity();
            ComponentsManager.AddComponent(entity3, new Position { X = 100f, Y = 200f, Z = 300f });
            ComponentsManager.AddComponent(entity3, new Velocity { X = 400f, Y = 500f, Z = 600f });
            
            // Create Entity4 only with Position
            Entity entity4 = World.CreateEntity();
            ComponentsManager.AddComponent(entity4, new Position { X = 1000f, Y = 2000f, Z = 3000f });
            
            // Call GetComponents<Position, Velocity>()
            var components = ComponentsManager.GetComponents<Position, Velocity>();
            
            // Verify returns span with three components
            AssertEquals(3, components.Length, "Should return 3 components");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Three Types - All Present")]
    public void Test_Query_010()
    {
        string testName = "Test_Query_010";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Position, Velocity, Health
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity1, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            
            // Create Entity2 with Position, Velocity (without Health)
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            ComponentsManager.AddComponent(entity2, new Velocity { X = 40f, Y = 50f, Z = 60f });
            
            // Create Entity3 with Position, Health (without Velocity)
            Entity entity3 = World.CreateEntity();
            ComponentsManager.AddComponent(entity3, new Position { X = 100f, Y = 200f, Z = 300f });
            ComponentsManager.AddComponent(entity3, new Health { Amount = 200f });
            
            // Call GetComponents<Position, Velocity, Health>()
            var components = ComponentsManager.GetComponents<Position, Velocity, Health>();
            
            // Verify returns only Entity1
            AssertEquals(1, components.Length, "Should return 1 component");
            AssertEquals(1f, components[0].X, "Position X should be 1f");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Three Types - No Match")]
    public void Test_Query_011()
    {
        string testName = "Test_Query_011";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Position, Velocity
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity1, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            // Create Entity2 with Position, Health
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            ComponentsManager.AddComponent(entity2, new Health { Amount = 100f });
            
            // Call GetComponents<Position, Velocity, Health>()
            var components = ComponentsManager.GetComponents<Position, Velocity, Health>();
            
            // Verify returns empty span
            AssertEquals(0, components.Length, "Should return 0 components");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Multiple Types - Empty Storage")]
    public void Test_Query_012()
    {
        string testName = "Test_Query_012";
        ExecuteTest(testName, () =>
        {
            // Don't add Velocity type components
            // Add several Position components
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            
            // Call GetComponents<Position, Velocity>()
            var components = ComponentsManager.GetComponents<Position, Velocity>();
            
            // Verify returns empty span (early exit)
            AssertEquals(0, components.Length, "Should return 0 components");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Multiple Types with World Parameter")]
    public void Test_Query_013()
    {
        string testName = "Test_Query_013";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity1 in testWorld with Position and Velocity
            Entity entity1 = World.CreateEntity(testWorld);
            ComponentsManager.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f }, testWorld);
            ComponentsManager.AddComponent(entity1, new Velocity { X = 4f, Y = 5f, Z = 6f }, testWorld);
            
            // Create Entity2 in global world with Position and Velocity
            Entity entity2 = World.CreateEntity(null);
            ComponentsManager.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f }, null);
            ComponentsManager.AddComponent(entity2, new Velocity { X = 40f, Y = 50f, Z = 60f }, null);
            
            // Call GetComponents<Position, Velocity>(testWorld)
            var components = ComponentsManager.GetComponents<Position, Velocity>(testWorld);
            
            // Verify returns only Entity1 from testWorld
            AssertEquals(1, components.Length, "Should return 1 component from testWorld");
            AssertEquals(1f, components[0].X, "Position X should be 1f");
        });
    }
    
    // ========== Core-009: ComponentsManager - GetComponentsWithout Query ==========
    
    [ContextMenu("Run Test: GetComponentsWithout Single Type (No Exclusions)")]
    public void Test_Query_014()
    {
        string testName = "Test_Query_014";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health to all three
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            
            // Call GetComponentsWithout<Health>()
            var components = ComponentsManager.GetComponentsWithout<Health>();
            
            // Verify returns all Health components (equivalent to GetComponents<Health>())
            AssertEquals(3, components.Length, "Should return 3 components");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout Two Types - Has T1 Not T2")]
    public void Test_Query_015()
    {
        string testName = "Test_Query_015";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Health (without Dead)
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            
            // Create Entity2 with Health and Dead
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity2, new Dead());
            
            // Create Entity3 with Health (without Dead)
            Entity entity3 = World.CreateEntity();
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            
            // Call GetComponentsWithout<Health, Dead>()
            var components = ComponentsManager.GetComponentsWithout<Health, Dead>();
            
            // Verify returns Health components of Entity1 and Entity3
            AssertEquals(2, components.Length, "Should return 2 components");
            
            // Verify result contains Entity1 and Entity3 components
            var amounts = new float[] { components[0].Amount, components[1].Amount };
            Assert(amounts.Contains(100f), "Should contain Entity1 health (100f)");
            Assert(amounts.Contains(300f), "Should contain Entity3 health (300f)");
            Assert(!amounts.Contains(200f), "Should not contain Entity2 health (200f)");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout Two Types - All Have T2")]
    public void Test_Query_016()
    {
        string testName = "Test_Query_016";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health and Dead to all three
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity1, new Dead());
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity2, new Dead());
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            ComponentsManager.AddComponent(entity3, new Dead());
            
            // Call GetComponentsWithout<Health, Dead>()
            var components = ComponentsManager.GetComponentsWithout<Health, Dead>();
            
            // Verify returns empty span
            AssertEquals(0, components.Length, "Should return 0 components");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout Three Types - Has T1 Not T2 Not T3")]
    public void Test_Query_017()
    {
        string testName = "Test_Query_017";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Health (without Dead, without Destroyed)
            Entity entity1 = World.CreateEntity();
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            
            // Create Entity2 with Health and Dead
            Entity entity2 = World.CreateEntity();
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity2, new Dead());
            
            // Create Entity3 with Health and Destroyed
            Entity entity3 = World.CreateEntity();
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            ComponentsManager.AddComponent(entity3, new Destroyed());
            
            // Create Entity4 with Health, Dead, Destroyed
            Entity entity4 = World.CreateEntity();
            ComponentsManager.AddComponent(entity4, new Health { Amount = 400f });
            ComponentsManager.AddComponent(entity4, new Dead());
            ComponentsManager.AddComponent(entity4, new Destroyed());
            
            // Call GetComponentsWithout<Health, Dead, Destroyed>()
            var components = ComponentsManager.GetComponentsWithout<Health, Dead, Destroyed>();
            
            // Verify returns only Entity1 Health component
            AssertEquals(1, components.Length, "Should return 1 component");
            AssertEquals(100f, components[0].Amount, "Health amount should be 100f");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout Three Types - No Match")]
    public void Test_Query_018()
    {
        string testName = "Test_Query_018";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health and Dead to all three
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity1, new Dead());
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity2, new Dead());
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            ComponentsManager.AddComponent(entity3, new Dead());
            
            // Call GetComponentsWithout<Health, Dead, Destroyed>()
            // This returns Health components for entities that have Health but NOT Dead and NOT Destroyed
            // Since all entities have Dead, they are all excluded
            var components = ComponentsManager.GetComponentsWithout<Health, Dead, Destroyed>();
            
            // Verify returns empty span (all entities have Dead, so they are excluded)
            AssertEquals(0, components.Length, "Should return 0 components (all entities have Dead, which is an exclusion)");
        });
    }
    
    [ContextMenu("Run Test: GetComponentsWithout with World Parameter")]
    public void Test_Query_019()
    {
        string testName = "Test_Query_019";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity1 in testWorld with Health (without Dead)
            Entity entity1 = World.CreateEntity(testWorld);
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f }, testWorld);
            
            // Create Entity2 in testWorld with Health and Dead
            Entity entity2 = World.CreateEntity(testWorld);
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f }, testWorld);
            ComponentsManager.AddComponent(entity2, new Dead(), testWorld);
            
            // Create Entity3 in global world with Health (without Dead)
            Entity entity3 = World.CreateEntity(null);
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f }, null);
            
            // Call GetComponentsWithout<Health, Dead>(testWorld)
            var components = ComponentsManager.GetComponentsWithout<Health, Dead>(testWorld);
            
            // Verify returns only Entity1 Health component from testWorld
            AssertEquals(1, components.Length, "Should return 1 component from testWorld");
            AssertEquals(100f, components[0].Amount, "Health amount should be 100f");
        });
    }
    
    // ========== Core-010: ComponentsManager - Deferred Component Modifications ==========
    
    [ContextMenu("Run Test: Modify Component Using ModifiableCollection")]
    public void Test_Modify_001()
    {
        string testName = "Test_Modify_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Health with Amount=100
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Use GetModifiableComponents<Health>()
            using (var components = ComponentsManager.GetModifiableComponents<Health>())
            {
                // Change Amount to 50 via ref
                if (components.Count > 0)
                {
                    components[0].Amount = 50f;
                }
            } // Dispose collection
            
            // Get component back
            var health = ComponentsManager.GetComponent<Health>(entity);
            
            // Verify changes applied after Dispose
            Assert(health.HasValue, "Health component should exist");
            AssertEquals(50f, health.Value.Amount, "Health Amount should be 50");
        });
    }
    
    [ContextMenu("Run Test: Modify Multiple Components")]
    public void Test_Modify_002()
    {
        string testName = "Test_Modify_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health to all with Amount=100
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity2, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity3, new Health { Amount = 100f });
            
            // Use GetModifiableComponents<Health>()
            using (var components = ComponentsManager.GetModifiableComponents<Health>())
            {
                // Change Amount for all three
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Amount = 50f + (i * 10f);
                }
            } // Dispose collection
            
            // Check all components
            var health1 = ComponentsManager.GetComponent<Health>(entity1);
            var health2 = ComponentsManager.GetComponent<Health>(entity2);
            var health3 = ComponentsManager.GetComponent<Health>(entity3);
            
            // Verify all changes applied
            Assert(health1.HasValue, "Entity1 health should exist");
            Assert(health2.HasValue, "Entity2 health should exist");
            Assert(health3.HasValue, "Entity3 health should exist");
            
            // Values may be in different order, so check that all expected values exist
            var amounts = new float[] { health1.Value.Amount, health2.Value.Amount, health3.Value.Amount };
            Assert(amounts.Contains(50f), "Should contain 50f");
            Assert(amounts.Contains(60f), "Should contain 60f");
            Assert(amounts.Contains(70f), "Should contain 70f");
        });
    }
    
    [ContextMenu("Run Test: Modify Component Without Dispose")]
    public void Test_Modify_003()
    {
        string testName = "Test_Modify_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Health with Amount=100
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Use GetModifiableComponents<Health>()
            var components = ComponentsManager.GetModifiableComponents<Health>();
            
            // Change Amount to 50
            if (components.Count > 0)
            {
                components[0].Amount = 50f;
            }
            
            // DON'T call Dispose
            // Get component back
            var health = ComponentsManager.GetComponent<Health>(entity);
            
            // Verify changes NOT applied (remains 100)
            Assert(health.HasValue, "Health component should exist");
            AssertEquals(100f, health.Value.Amount, "Health Amount should remain 100 (not disposed)");
            
            // Dispose now to clean up
            components.Dispose();
        });
    }
    
    [ContextMenu("Run Test: Modify Component Safe Iteration")]
    public void Test_Modify_004()
    {
        string testName = "Test_Modify_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health to all
            ComponentsManager.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity2, new Health { Amount = 200f });
            ComponentsManager.AddComponent(entity3, new Health { Amount = 300f });
            
            // Use GetModifiableComponents<Health>()
            using (var components = ComponentsManager.GetModifiableComponents<Health>())
            {
                // Iterate and modify all components
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Amount -= 10f;
                }
            } // Dispose collection
            
            // Verify all components modified correctly
            var health1 = ComponentsManager.GetComponent<Health>(entity1);
            var health2 = ComponentsManager.GetComponent<Health>(entity2);
            var health3 = ComponentsManager.GetComponent<Health>(entity3);
            
            Assert(health1.HasValue, "Entity1 health should exist");
            Assert(health2.HasValue, "Entity2 health should exist");
            Assert(health3.HasValue, "Entity3 health should exist");
            
            // All should be reduced by 10
            var amounts = new float[] { health1.Value.Amount, health2.Value.Amount, health3.Value.Amount };
            Assert(amounts.Contains(90f), "Should contain 90f");
            Assert(amounts.Contains(190f), "Should contain 190f");
            Assert(amounts.Contains(290f), "Should contain 290f");
        });
    }
    
    [ContextMenu("Run Test: Modify Component with World Parameter")]
    public void Test_Modify_005()
    {
        string testName = "Test_Modify_005";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity in testWorld
            Entity entity = World.CreateEntity(testWorld);
            
            // Add Health to testWorld and to global world
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f }, testWorld);
            ComponentsManager.AddComponent(entity, new Health { Amount = 200f }, null);
            
            // Use GetModifiableComponents<Health>(testWorld)
            using (var components = ComponentsManager.GetModifiableComponents<Health>(testWorld))
            {
                // Change Amount
                if (components.Count > 0)
                {
                    components[0].Amount = 50f;
                }
            } // Dispose collection
            
            // Check components in both worlds
            var healthTest = ComponentsManager.GetComponent<Health>(entity, testWorld);
            var healthGlobal = ComponentsManager.GetComponent<Health>(entity, null);
            
            // Verify changes applied only in specified world
            Assert(healthTest.HasValue, "Health should exist in testWorld");
            AssertEquals(50f, healthTest.Value.Amount, "Health Amount in testWorld should be 50");
            Assert(healthGlobal.HasValue, "Health should exist in global world");
            AssertEquals(200f, healthGlobal.Value.Amount, "Health Amount in global world should remain 200");
        });
    }
    
    [ContextMenu("Run Test: Access Disposed Collection Throws Exception")]
    public void Test_Modify_006()
    {
        string testName = "Test_Modify_006";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Health
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Use GetModifiableComponents<Health>()
            var components = ComponentsManager.GetModifiableComponents<Health>();
            
            // Dispose collection
            components.Dispose();
            
            // Attempt to access component through collection
            bool exceptionThrown = false;
            try
            {
                var _ = components[0];
            }
            catch (ObjectDisposedException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ObjectDisposedException should be thrown");
        });
    }
    
    // ========== Core-011: Entity Pool Implementation ==========
    
    [ContextMenu("Run Test: Allocate New Entity")]
    public void Test_Pool_001()
    {
        string testName = "Test_Pool_001";
        ExecuteTest(testName, () =>
        {
            // Call EntitiesManager.Allocate()
            Entity entity = EntitiesManager.Allocate();
            
            // Verify returns valid Entity
            Assert(entity.IsValid, "Entity should be valid");
            Assert(entity.Id >= 0, "Entity.Id should be >= 0");
            Assert(entity.Generation >= 0, "Entity.Generation should be >= 0");
        });
    }
    
    [ContextMenu("Run Test: Allocate Multiple Entities")]
    public void Test_Pool_002()
    {
        string testName = "Test_Pool_002";
        ExecuteTest(testName, () =>
        {
            // Call EntitiesManager.Allocate() three times
            Entity entity1 = EntitiesManager.Allocate();
            Entity entity2 = EntitiesManager.Allocate();
            Entity entity3 = EntitiesManager.Allocate();
            
            // Verify ID uniqueness
            Assert(entity1.Id != entity2.Id, "Entity1.Id != Entity2.Id");
            Assert(entity2.Id != entity3.Id, "Entity2.Id != Entity3.Id");
            Assert(entity1.Id != entity3.Id, "Entity1.Id != Entity3.Id");
        });
    }
    
    [ContextMenu("Run Test: Deallocate Entity")]
    public void Test_Pool_003()
    {
        string testName = "Test_Pool_003";
        ExecuteTest(testName, () =>
        {
            // Allocate Entity
            Entity entity = EntitiesManager.Allocate();
            
            // Deallocate Entity
            bool deallocated = EntitiesManager.Deallocate(entity);
            
            // Verify that Entity is no longer allocated
            Assert(deallocated, "Deallocate should return true");
            Assert(!EntitiesManager.IsAllocated(entity), "Entity should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: Deallocate Invalid Entity")]
    public void Test_Pool_004()
    {
        string testName = "Test_Pool_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity.Invalid
            Entity invalid = Entity.Invalid;
            
            // Attempt to deallocate
            bool deallocated = EntitiesManager.Deallocate(invalid);
            
            // Verify returns false
            Assert(!deallocated, "Deallocate should return false");
        });
    }
    
    [ContextMenu("Run Test: Entity ID Recycling")]
    public void Test_Pool_005()
    {
        string testName = "Test_Pool_005";
        ExecuteTest(testName, () =>
        {
            // Allocate Entity1
            Entity entity1 = EntitiesManager.Allocate();
            
            // Remember Entity1 ID
            int entity1Id = entity1.Id;
            int entity1Gen = entity1.Generation;
            
            // Deallocate Entity1
            // This increments generation in _generations dictionary to invalidate old references
            EntitiesManager.Deallocate(entity1);
            
            // Allocate Entity2
            Entity entity2 = EntitiesManager.Allocate();
            
            // Verify that Entity2 has same ID but different Generation
            // Note: ID recycling may not happen immediately if pool has other available IDs
            // So we check that if ID is reused, generation is increased
            // Generation is incremented TWICE: once during Deallocate, once during Allocate (when reusing)
            if (entity2.Id == entity1Id)
            {
                AssertEquals(entity1Gen + 2, entity2.Generation, "Generation should be incremented twice: once on deallocation, once on reallocation");
            }
        });
    }
    
    [ContextMenu("Run Test: Generation Safety")]
    public void Test_Pool_006()
    {
        string testName = "Test_Pool_006";
        ExecuteTest(testName, () =>
        {
            // Allocate Entity1
            Entity entity1 = EntitiesManager.Allocate();
            
            // Remember Entity1 completely (ID and Generation)
            Entity oldEntity1 = entity1;
            
            // Deallocate Entity1
            EntitiesManager.Deallocate(entity1);
            
            // Allocate Entity2 (reuses ID)
            Entity entity2 = EntitiesManager.Allocate();
            
            // Verify that old Entity1 is not equal to Entity2
            Assert(oldEntity1 != entity2, "Old Entity1 should not equal Entity2");
            Assert(!EntitiesManager.IsAllocated(oldEntity1), "Old Entity1 should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: IsAllocated Check")]
    public void Test_Pool_007()
    {
        string testName = "Test_Pool_007";
        ExecuteTest(testName, () =>
        {
            // Allocate Entity
            Entity entity = EntitiesManager.Allocate();
            
            // Check IsAllocated
            bool allocated = EntitiesManager.IsAllocated(entity);
            
            // Deallocate Entity
            EntitiesManager.Deallocate(entity);
            
            // Check IsAllocated again
            bool allocatedAfter = EntitiesManager.IsAllocated(entity);
            
            // Verify IsAllocated correctly reflects state
            Assert(allocated, "Entity should be allocated after allocation");
            Assert(!allocatedAfter, "Entity should not be allocated after deallocation");
        });
    }
    
    [ContextMenu("Run Test: GetAllocatedCount")]
    public void Test_Pool_008()
    {
        string testName = "Test_Pool_008";
        ExecuteTest(testName, () =>
        {
            // Get initial count
            int initialCount = EntitiesManager.GetAllocatedCount();
            
            // Allocate 3 entities
            Entity entity1 = EntitiesManager.Allocate();
            Entity entity2 = EntitiesManager.Allocate();
            Entity entity3 = EntitiesManager.Allocate();
            
            // Get new count
            int newCount = EntitiesManager.GetAllocatedCount();
            
            // Deallocate 1 entity
            EntitiesManager.Deallocate(entity1);
            
            // Get final count
            int finalCount = EntitiesManager.GetAllocatedCount();
            
            // Verify count correctly tracks allocated entities
            AssertEquals(0, initialCount, "Initial count should be 0");
            AssertEquals(3, newCount, "New count should be 3");
            AssertEquals(2, finalCount, "Final count should be 2");
        });
    }
    
    [ContextMenu("Run Test: GetAvailableCount")]
    public void Test_Pool_009()
    {
        string testName = "Test_Pool_009";
        ExecuteTest(testName, () =>
        {
            // Get initial available count
            int initialAvailable = EntitiesManager.GetAvailableCount();
            
            // Allocate 3 entities
            Entity entity1 = EntitiesManager.Allocate();
            Entity entity2 = EntitiesManager.Allocate();
            Entity entity3 = EntitiesManager.Allocate();
            
            // Deallocate 2 entities
            EntitiesManager.Deallocate(entity1);
            EntitiesManager.Deallocate(entity2);
            
            // Get available count
            int finalAvailable = EntitiesManager.GetAvailableCount();
            
            // Verify available count correctly tracks deallocated IDs
            AssertEquals(0, initialAvailable, "Initial available should be 0");
            AssertEquals(2, finalAvailable, "Final available should be 2");
        });
    }
    
    [ContextMenu("Run Test: Clear Pool")]
    public void Test_Pool_010()
    {
        string testName = "Test_Pool_010";
        ExecuteTest(testName, () =>
        {
            // Allocate several entities
            Entity entity1 = EntitiesManager.Allocate();
            Entity entity2 = EntitiesManager.Allocate();
            Entity entity3 = EntitiesManager.Allocate();
            
            // Deallocate some
            EntitiesManager.Deallocate(entity1);
            
            // Clear pool
            EntitiesManager.Clear();
            
            // Check counts
            int allocatedCount = EntitiesManager.GetAllocatedCount();
            int availableCount = EntitiesManager.GetAvailableCount();
            
            // Verify pool cleared, counts reset
            AssertEquals(0, allocatedCount, "Allocated count should be 0");
            AssertEquals(0, availableCount, "Available count should be 0");
        });
    }
    
    [ContextMenu("Run Test: World Scoped Pools")]
    public void Test_Pool_011()
    {
        string testName = "Test_Pool_011";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Allocate Entity1 in global world
            Entity entity1 = EntitiesManager.Allocate(null);
            
            // Allocate Entity2 in testWorld
            Entity entity2 = EntitiesManager.Allocate(testWorld);
            
            // Check counts in each world
            int globalCount = EntitiesManager.GetAllocatedCount(null);
            int testCount = EntitiesManager.GetAllocatedCount(testWorld);
            
            // Verify pools isolated by worlds
            AssertEquals(1, globalCount, "Global world should have 1 allocated entity");
            AssertEquals(1, testCount, "TestWorld should have 1 allocated entity");
        });
    }
    
    [ContextMenu("Run Test: Deallocate Twice")]
    public void Test_Pool_012()
    {
        string testName = "Test_Pool_012";
        ExecuteTest(testName, () =>
        {
            // Allocate Entity
            Entity entity = EntitiesManager.Allocate();
            
            // Deallocate Entity
            bool firstDealloc = EntitiesManager.Deallocate(entity);
            
            // Attempt to deallocate Entity again
            bool secondDealloc = EntitiesManager.Deallocate(entity);
            
            // Verify second deallocation returns false
            Assert(firstDealloc, "First deallocation should return true");
            Assert(!secondDealloc, "Second deallocation should return false");
        });
    }
    
    // ========== Core-012: Entity Creation/Destruction API ==========
    
    [ContextMenu("Run Test: CreateEntity")]
    public void Test_World_001()
    {
        string testName = "Test_World_001";
        ExecuteTest(testName, () =>
        {
            // Call World.CreateEntity()
            Entity entity = World.CreateEntity();
            
            // Verify returns valid Entity
            Assert(entity.IsValid, "Entity should be valid");
            Assert(EntitiesManager.IsAllocated(entity), "Entity should be allocated");
        });
    }
    
    [ContextMenu("Run Test: CreateEntity Multiple Times")]
    public void Test_World_002()
    {
        string testName = "Test_World_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3 through World.CreateEntity()
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Verify uniqueness
            Assert(entity1 != entity2, "Entity1 != Entity2");
            Assert(entity2 != entity3, "Entity2 != Entity3");
            Assert(entity1 != entity3, "Entity1 != Entity3");
        });
    }
    
    [ContextMenu("Run Test: CreateEntity with World Parameter")]
    public void Test_World_003()
    {
        string testName = "Test_World_003";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity1 through World.CreateEntity(null)
            Entity entity1 = World.CreateEntity(null);
            
            // Create Entity2 through World.CreateEntity(testWorld)
            Entity entity2 = World.CreateEntity(testWorld);
            
            // Verify that they are in different pools
            Assert(EntitiesManager.IsAllocated(entity1, null), "Entity1 should be allocated in global world");
            Assert(EntitiesManager.IsAllocated(entity2, testWorld), "Entity2 should be allocated in testWorld");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Without Components")]
    public void Test_World_004()
    {
        string testName = "Test_World_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // Verify that entity is destroyed
            Assert(destroyed, "DestroyEntity should return true");
            Assert(!EntitiesManager.IsAllocated(entity), "Entity should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity With Components")]
    public void Test_World_005()
    {
        string testName = "Test_World_005";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position, Velocity, Health
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // Verify that all components are removed
            Assert(destroyed, "DestroyEntity should return true");
            var position = ComponentsManager.GetComponent<Position>(entity);
            var velocity = ComponentsManager.GetComponent<Velocity>(entity);
            var health = ComponentsManager.GetComponent<Health>(entity);
            Assert(!position.HasValue, "Position should be removed");
            Assert(!velocity.HasValue, "Velocity should be removed");
            Assert(!health.HasValue, "Health should be removed");
            Assert(!EntitiesManager.IsAllocated(entity), "Entity should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Multiple Components")]
    public void Test_World_006()
    {
        string testName = "Test_World_006";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add 10 different component types
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 });
            ComponentsManager.AddComponent(entity, new Dead());
            ComponentsManager.AddComponent(entity, new Destroyed());
            // We only have 6 component types, so we'll use what we have
            // In a real scenario, there would be 10 different types
            
            // Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // Verify that all components are removed
            Assert(destroyed, "DestroyEntity should return true");
            Assert(!ComponentsManager.GetComponent<Position>(entity).HasValue, "Position should be removed");
            Assert(!ComponentsManager.GetComponent<Velocity>(entity).HasValue, "Velocity should be removed");
            Assert(!ComponentsManager.GetComponent<Health>(entity).HasValue, "Health should be removed");
            Assert(!ComponentsManager.GetComponent<TestComponent>(entity).HasValue, "TestComponent should be removed");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Invalid Entity")]
    public void Test_World_007()
    {
        string testName = "Test_World_007";
        ExecuteTest(testName, () =>
        {
            // Attempt to destroy Entity.Invalid
            bool destroyed = World.DestroyEntity(Entity.Invalid);
            
            // Verify returns false
            Assert(!destroyed, "DestroyEntity should return false for invalid entity");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Twice")]
    public void Test_World_008()
    {
        string testName = "Test_World_008";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Destroy Entity
            bool firstDestroy = World.DestroyEntity(entity);
            
            // Attempt to destroy Entity again
            bool secondDestroy = World.DestroyEntity(entity);
            
            // Verify second destruction returns false
            Assert(firstDestroy, "First destruction should return true");
            Assert(!secondDestroy, "Second destruction should return false");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity with World Parameter")]
    public void Test_World_009()
    {
        string testName = "Test_World_009";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create Entity in testWorld
            Entity entity = World.CreateEntity(testWorld);
            
            // Add components to testWorld and to global world
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 1 }, testWorld);
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 2 }, null);
            
            // Destroy Entity in testWorld
            bool destroyed = World.DestroyEntity(entity, testWorld);
            
            // Check components in both worlds
            var compTest = ComponentsManager.GetComponent<TestComponent>(entity, testWorld);
            var compGlobal = ComponentsManager.GetComponent<TestComponent>(entity, null);
            
            // Verify components removed only from specified world, entity deallocated
            Assert(destroyed, "DestroyEntity should return true");
            Assert(!compTest.HasValue, "Component should be removed from testWorld");
            Assert(compGlobal.HasValue, "Component should remain in global world");
            Assert(!EntitiesManager.IsAllocated(entity, testWorld), "Entity should not be allocated in testWorld");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Component Cleanup Integration")]
    public void Test_World_010()
    {
        string testName = "Test_World_010";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add components of different types
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Destroy Entity
            World.DestroyEntity(entity);
            
            // Verify that GetComponents for all types don't contain this entity
            var positions = ComponentsManager.GetComponents<Position>();
            var velocities = ComponentsManager.GetComponents<Velocity>();
            var healths = ComponentsManager.GetComponents<Health>();
            
            // Since this was the only entity, all should be empty
            AssertEquals(0, positions.Length, "Positions should be empty");
            AssertEquals(0, velocities.Length, "Velocities should be empty");
            AssertEquals(0, healths.Length, "Healths should be empty");
        });
    }
    
    [ContextMenu("Run Test: CreateDestroyCreate Cycle")]
    public void Test_World_011()
    {
        string testName = "Test_World_011";
        ExecuteTest(testName, () =>
        {
            // Create Entity1
            Entity entity1 = World.CreateEntity();
            
            // Add components
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // Destroy Entity1
            World.DestroyEntity(entity1);
            
            // Create Entity2
            Entity entity2 = World.CreateEntity();
            
            // Verify that Entity2 can use recycled ID
            // Note: ID recycling may not happen immediately
            // Components should not be recycled
            var component = ComponentsManager.GetComponent<TestComponent>(entity2);
            Assert(!component.HasValue, "Entity2 should not have recycled components");
        });
    }
    
    // ========== Integration Tests ==========
    
    [ContextMenu("Run Test: Full Entity Lifecycle")]
    public void Test_Integration_001()
    {
        string testName = "Test_Integration_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add several components
            ComponentsManager.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            ComponentsManager.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            ComponentsManager.AddComponent(entity, new Health { Amount = 100f });
            
            // Modify components
            using (var components = ComponentsManager.GetModifiableComponents<Health>())
            {
                if (components.Count > 0)
                {
                    components[0].Amount = 90f;
                }
            }
            
            // Remove one component
            ComponentsManager.RemoveComponent<Velocity>(entity);
            
            // Add new component
            ComponentsManager.AddComponent(entity, new TestComponent { Value = 42 });
            
            // Destroy entity
            World.DestroyEntity(entity);
            
            // Verify final state
            Assert(!EntitiesManager.IsAllocated(entity), "Entity should not be allocated");
            var position = ComponentsManager.GetComponent<Position>(entity);
            Assert(!position.HasValue, "Position should be removed");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Isolation")]
    public void Test_Integration_002()
    {
        string testName = "Test_Integration_002";
        ExecuteTest(testName, () =>
        {
            // Create World1, World2, World3
            World world1 = new World("World1");
            World world2 = new World("World2");
            World world3 = new World("World3");
            
            // Create entities in each world
            Entity entity1 = World.CreateEntity(world1);
            Entity entity2 = World.CreateEntity(world2);
            Entity entity3 = World.CreateEntity(world3);
            
            // Add components in different worlds
            ComponentsManager.AddComponent(entity1, new TestComponent { Value = 1 }, world1);
            ComponentsManager.AddComponent(entity2, new TestComponent { Value = 2 }, world2);
            ComponentsManager.AddComponent(entity3, new TestComponent { Value = 3 }, world3);
            
            // Execute queries in each world
            var comps1 = ComponentsManager.GetComponents<TestComponent>(world1);
            var comps2 = ComponentsManager.GetComponents<TestComponent>(world2);
            var comps3 = ComponentsManager.GetComponents<TestComponent>(world3);
            
            // Verify worlds are completely isolated
            AssertEquals(1, comps1.Length, "World1 should have 1 component");
            AssertEquals(1, comps2.Length, "World2 should have 1 component");
            AssertEquals(1, comps3.Length, "World3 should have 1 component");
            
            // Destroy entities in different worlds
            World.DestroyEntity(entity1, world1);
            World.DestroyEntity(entity2, world2);
            World.DestroyEntity(entity3, world3);
            
            // Verify isolation maintained
            Assert(!EntitiesManager.IsAllocated(entity1, world1), "Entity1 should not be allocated");
            Assert(!EntitiesManager.IsAllocated(entity2, world2), "Entity2 should not be allocated");
            Assert(!EntitiesManager.IsAllocated(entity3, world3), "Entity3 should not be allocated");
        });
    }
    
    [ContextMenu("Run Test: Component Query Performance")]
    public void Test_Integration_003()
    {
        string testName = "Test_Integration_003";
        ExecuteTest(testName, () =>
        {
            // Create 1000 entities
            Entity[] entities = new Entity[1000];
            for (int i = 0; i < 1000; i++)
            {
                entities[i] = World.CreateEntity();
            }
            
            // Add components to different entities
            // Position: entities 0-499 (500 entities)
            for (int i = 0; i < 500; i++)
            {
                ComponentsManager.AddComponent(entities[i], new Position { X = i, Y = i, Z = i });
            }
            
            // Velocity: entities 0-299 (300 entities) - all of these also have Position
            for (int i = 0; i < 300; i++)
            {
                ComponentsManager.AddComponent(entities[i], new Velocity { X = i, Y = i, Z = i });
            }
            
            // Health: entities 0-199 (200 entities) - all of these also have Position and Velocity
            for (int i = 0; i < 200; i++)
            {
                ComponentsManager.AddComponent(entities[i], new Health { Amount = i });
            }
            
            // Execute various queries
            var positions = ComponentsManager.GetComponents<Position>();
            var velocities = ComponentsManager.GetComponents<Velocity>();
            var healths = ComponentsManager.GetComponents<Health>();
            var posVel = ComponentsManager.GetComponents<Position, Velocity>();
            var posVelHealth = ComponentsManager.GetComponents<Position, Velocity, Health>();
            
            // Verify queries complete successfully
            // Position AND Velocity: entities 0-299 = 300 entities (all Velocity entities also have Position)
            // Position AND Velocity AND Health: entities 0-199 = 200 entities (all Health entities also have Position and Velocity)
            AssertEquals(500, positions.Length, "Should have 500 positions");
            AssertEquals(300, velocities.Length, "Should have 300 velocities");
            AssertEquals(200, healths.Length, "Should have 200 healths");
            AssertEquals(300, posVel.Length, "Should have 300 entities with Position and Velocity (intersection of 0-499 and 0-299 = 0-299)");
            AssertEquals(200, posVelHealth.Length, "Should have 200 entities with all three");
        });
    }
}
