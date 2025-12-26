using UnityEngine;
using ArtyECS.Core;
using System;

/// <summary>
/// Test class for Entity ↔ GameObject linking functionality (Link-000 through Link-006).
/// </summary>
public class LinkTests : TestBase
{
    // ========== Link-002: CreateEntity GameObject Overload ==========
    
    [ContextMenu("Run Test: CreateEntity with GameObject")]
    public void Test_Link_001()
    {
        string testName = "Test_Link_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 2. Create entity linked to GameObject
            Entity entity = World.CreateEntity(gameObject);
            
            // 3. Verify entity is valid
            Assert(entity.IsValid, "Entity should be valid");
            
            // 4. Verify GameObject is linked
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            AssertEquals(gameObject, linkedGO, "Linked GameObject should match original");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: CreateEntity with null GameObject throws exception")]
    public void Test_Link_002()
    {
        string testName = "Test_Link_002";
        ExecuteTest(testName, () =>
        {
            // 1. Try to create entity with null GameObject
            bool exceptionThrown = false;
            try
            {
                World.CreateEntity(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            
            // 2. Verify exception was thrown
            Assert(exceptionThrown, "ArgumentNullException should be thrown for null GameObject");
        });
    }
    
    [ContextMenu("Run Test: CreateEntity with GameObject via WorldInstance")]
    public void Test_Link_003()
    {
        string testName = "Test_Link_003";
        ExecuteTest(testName, () =>
        {
            // 1. Get world instance
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 3. Create entity linked to GameObject via WorldInstance
            Entity entity = world.CreateEntity(gameObject);
            
            // 4. Verify entity is valid
            Assert(entity.IsValid, "Entity should be valid");
            
            // 5. Verify GameObject is linked
            GameObject linkedGO = world.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            AssertEquals(gameObject, linkedGO, "Linked GameObject should match original");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    // ========== Link-003: GetGameObject and GetEntity Public Methods ==========
    
    [ContextMenu("Run Test: GetGameObject returns linked GameObject")]
    public void Test_Link_004()
    {
        string testName = "Test_Link_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Get GameObject via GetGameObject
            GameObject retrievedGO = World.GetGameObject(entity);
            
            // 3. Verify GameObject matches
            AssertNotNull(retrievedGO, "GetGameObject should return non-null");
            AssertEquals(gameObject, retrievedGO, "Retrieved GameObject should match original");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: GetGameObject returns null for unlinked entity")]
    public void Test_Link_005()
    {
        string testName = "Test_Link_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity without GameObject
            Entity entity = World.CreateEntity();
            
            // 2. Get GameObject for unlinked entity
            GameObject retrievedGO = World.GetGameObject(entity);
            
            // 3. Verify null is returned
            AssertNull(retrievedGO, "GetGameObject should return null for unlinked entity");
        });
    }
    
    [ContextMenu("Run Test: GetEntity returns linked entity")]
    public void Test_Link_006()
    {
        string testName = "Test_Link_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Get entity via GetEntity
            Entity? retrievedEntity = World.GetEntity(gameObject);
            
            // 3. Verify entity matches
            Assert(retrievedEntity.HasValue, "GetEntity should return non-null entity");
            AssertEquals(entity, retrievedEntity.Value, "Retrieved entity should match original");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: GetEntity returns null for unlinked GameObject")]
    public void Test_Link_007()
    {
        string testName = "Test_Link_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject without entity
            GameObject gameObject = new GameObject("TestObject");
            
            // 2. Get entity for unlinked GameObject
            Entity? retrievedEntity = World.GetEntity(gameObject);
            
            // 3. Verify null is returned
            Assert(!retrievedEntity.HasValue, "GetEntity should return null for unlinked GameObject");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: GetEntity returns null for null GameObject")]
    public void Test_Link_008()
    {
        string testName = "Test_Link_008";
        ExecuteTest(testName, () =>
        {
            // 1. Get entity for null GameObject
            Entity? retrievedEntity = World.GetEntity(null);
            
            // 2. Verify null is returned
            Assert(!retrievedEntity.HasValue, "GetEntity should return null for null GameObject");
        });
    }
    
    [ContextMenu("Run Test: GetGameObject handles destroyed GameObject")]
    public void Test_Link_009()
    {
        string testName = "Test_Link_009";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Destroy GameObject
            UnityEngine.Object.DestroyImmediate(gameObject);
            
            // 3. Get GameObject for entity with destroyed GameObject
            GameObject retrievedGO = World.GetGameObject(entity);
            
            // 4. Verify null is returned (link should be cleaned up)
            AssertNull(retrievedGO, "GetGameObject should return null for destroyed GameObject");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    // ========== Link-004: DestroyEntity Automatic Cleanup ==========
    
    [ContextMenu("Run Test: DestroyEntity automatically unlinks GameObject")]
    public void Test_Link_010()
    {
        string testName = "Test_Link_010";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Verify link exists
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked before destruction");
            
            // 3. Store gameObjectId before destruction for verification
            int gameObjectId = gameObject.GetInstanceID();
            
            // 4. Destroy entity (this will also destroy the GameObject)
            bool destroyed = World.DestroyEntity(entity);
            Assert(destroyed, "Entity should be destroyed");
            
            // 5. Verify link is removed from entity side
            // After DestroyEntity, UnlinkEntity should have removed the dictionary entry
            GameObject retrievedGO = World.GetGameObject(entity);
            AssertNull(retrievedGO, "GetGameObject should return null after entity destruction");
            
            // Cleanup - GameObject is already destroyed by DestroyEntity, but ensure it's cleaned up
            if (gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity unlinks GameObject via WorldInstance")]
    public void Test_Link_011()
    {
        string testName = "Test_Link_011";
        ExecuteTest(testName, () =>
        {
            // 1. Get world instance
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = world.CreateEntity(gameObject);
            
            // 3. Verify link exists
            GameObject linkedGO = world.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked before destruction");
            
            // 4. Destroy entity via WorldInstance (this will also destroy the GameObject)
            bool destroyed = world.DestroyEntity(entity);
            Assert(destroyed, "Entity should be destroyed");
            
            // 5. Verify link is removed
            GameObject retrievedGO = world.GetGameObject(entity);
            AssertNull(retrievedGO, "GetGameObject should return null after entity destruction");
            
            // Cleanup - GameObject is already destroyed by DestroyEntity, but ensure it's cleaned up
            if (gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        });
    }
    
    // ========== Link-001: LinkEntity and UnlinkEntity Internal Methods ==========
    
    [ContextMenu("Run Test: Entity can be relinked to different GameObject")]
    public void Test_Link_012()
    {
        string testName = "Test_Link_012";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity and first GameObject
            GameObject gameObject1 = new GameObject("TestObject1");
            Entity entity = World.CreateEntity(gameObject1);
            
            // 2. Verify first link
            GameObject linkedGO1 = World.GetGameObject(entity);
            AssertEquals(gameObject1, linkedGO1, "First GameObject should be linked");
            
            // 3. Destroy entity to unlink first GameObject (GameObject1 is destroyed by DestroyEntity)
            World.DestroyEntity(entity);
            
            // 4. Create second GameObject and new entity
            GameObject gameObject2 = new GameObject("TestObject2");
            Entity newEntity = World.CreateEntity(gameObject2);
            
            // 5. Verify second link
            GameObject linkedGO2 = World.GetGameObject(newEntity);
            AssertEquals(gameObject2, linkedGO2, "Second GameObject should be linked");
            
            // Cleanup
            // Note: gameObject1 is already destroyed by DestroyEntity
            UnityEngine.Object.DestroyImmediate(gameObject2);
        });
    }
    
    [ContextMenu("Run Test: Same GameObject cannot be linked to different entities")]
    public void Test_Link_013()
    {
        string testName = "Test_Link_013";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 2. Create first entity linked to GameObject
            Entity entity1 = World.CreateEntity(gameObject);
            
            // 3. Try to create second entity linked to same GameObject
            bool exceptionThrown = false;
            try
            {
                Entity entity2 = World.CreateEntity(gameObject);
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            
            // 4. Verify exception was thrown
            Assert(exceptionThrown, "InvalidOperationException should be thrown when linking same GameObject to different entity");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    // ========== Link-005: World Static Class Methods ==========
    
    [ContextMenu("Run Test: World.CreateEntity with GameObject")]
    public void Test_Link_014()
    {
        string testName = "Test_Link_014";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 2. Create entity via World static method
            Entity entity = World.CreateEntity(gameObject);
            
            // 3. Verify entity is valid and linked
            Assert(entity.IsValid, "Entity should be valid");
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            AssertEquals(gameObject, linkedGO, "Linked GameObject should match");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: World.GetGameObject")]
    public void Test_Link_015()
    {
        string testName = "Test_Link_015";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Get GameObject via World static method
            GameObject retrievedGO = World.GetGameObject(entity);
            
            // 3. Verify GameObject matches
            AssertNotNull(retrievedGO, "World.GetGameObject should return non-null");
            AssertEquals(gameObject, retrievedGO, "Retrieved GameObject should match");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: World.GetEntity")]
    public void Test_Link_016()
    {
        string testName = "Test_Link_016";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Get entity via World static method
            Entity? retrievedEntity = World.GetEntity(gameObject);
            
            // 3. Verify entity matches
            Assert(retrievedEntity.HasValue, "World.GetEntity should return non-null");
            AssertEquals(entity, retrievedEntity.Value, "Retrieved entity should match");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    // ========== Link-006: TransformSyncSystem Example ==========
    
    [ContextMenu("Run Test: TransformSyncSystem synchronizes Position to Transform")]
    public void Test_Link_017()
    {
        string testName = "Test_Link_017";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Add Position component
            Position position = new Position { X = 10f, Y = 20f, Z = 30f };
            World.AddComponent(entity, position);
            
            // 3. Create and register TransformSyncSystem
            TransformSyncSystem syncSystem = new TransformSyncSystem();
            World.AddToUpdate(syncSystem);
            
            // 4. Execute system once
            World.ExecuteOnce(syncSystem);
            
            // 5. Verify Transform.position matches Position component
            Vector3 transformPos = gameObject.transform.position;
            AssertEquals(10f, transformPos.x, "Transform.position.x should match Position.X");
            AssertEquals(20f, transformPos.y, "Transform.position.y should match Position.Y");
            AssertEquals(30f, transformPos.z, "Transform.position.z should match Position.Z");
            
            // 6. Update Position component
            ref Position posRef = ref World.GetModifiableComponent<Position>(entity);
            posRef.X = 100f;
            posRef.Y = 200f;
            posRef.Z = 300f;
            
            // 7. Execute system again
            World.ExecuteOnce(syncSystem);
            
            // 8. Verify Transform.position is updated
            transformPos = gameObject.transform.position;
            AssertEquals(100f, transformPos.x, "Transform.position.x should be updated");
            AssertEquals(200f, transformPos.y, "Transform.position.y should be updated");
            AssertEquals(300f, transformPos.z, "Transform.position.z should be updated");
            
            // Cleanup
            World.RemoveFromUpdate(syncSystem);
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: TransformSyncSystem handles entities without GameObject")]
    public void Test_Link_018()
    {
        string testName = "Test_Link_018";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity without GameObject
            Entity entity = World.CreateEntity();
            
            // 2. Add Position component
            Position position = new Position { X = 10f, Y = 20f, Z = 30f };
            World.AddComponent(entity, position);
            
            // 3. Create and register TransformSyncSystem
            TransformSyncSystem syncSystem = new TransformSyncSystem();
            World.AddToUpdate(syncSystem);
            
            // 4. Execute system (should not throw exception)
            World.ExecuteOnce(syncSystem);
            
            // 5. Test passes if no exception is thrown
            Assert(true, "System should handle entities without GameObject gracefully");
            
            // Cleanup
            World.RemoveFromUpdate(syncSystem);
        });
    }
    
    [ContextMenu("Run Test: TransformSyncSystem handles multiple entities")]
    public void Test_Link_019()
    {
        string testName = "Test_Link_019";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple GameObjects and entities
            GameObject gameObject1 = new GameObject("TestObject1");
            GameObject gameObject2 = new GameObject("TestObject2");
            GameObject gameObject3 = new GameObject("TestObject3");
            
            Entity entity1 = World.CreateEntity(gameObject1);
            Entity entity2 = World.CreateEntity(gameObject2);
            Entity entity3 = World.CreateEntity(gameObject3);
            
            // 2. Add Position components with different values
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            World.AddComponent(entity3, new Position { X = 100f, Y = 200f, Z = 300f });
            
            // 3. Create and register TransformSyncSystem
            TransformSyncSystem syncSystem = new TransformSyncSystem();
            World.AddToUpdate(syncSystem);
            
            // 4. Execute system
            World.ExecuteOnce(syncSystem);
            
            // 5. Verify all Transforms are synchronized
            AssertEquals(1f, gameObject1.transform.position.x, "GameObject1 position should be synchronized");
            AssertEquals(10f, gameObject2.transform.position.x, "GameObject2 position should be synchronized");
            AssertEquals(100f, gameObject3.transform.position.x, "GameObject3 position should be synchronized");
            
            // Cleanup
            World.RemoveFromUpdate(syncSystem);
            UnityEngine.Object.DestroyImmediate(gameObject1);
            UnityEngine.Object.DestroyImmediate(gameObject2);
            UnityEngine.Object.DestroyImmediate(gameObject3);
        });
    }
    
    // ========== Additional Integration Tests ==========
    
    [ContextMenu("Run Test: Link works across different worlds")]
    public void Test_Link_020()
    {
        string testName = "Test_Link_020";
        ExecuteTest(testName, () =>
        {
            // 1. Create two world instances
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create GameObjects
            GameObject gameObject1 = new GameObject("TestObject1");
            GameObject gameObject2 = new GameObject("TestObject2");
            
            // 3. Create entities in different worlds
            Entity entity1 = world1.CreateEntity(gameObject1);
            Entity entity2 = world2.CreateEntity(gameObject2);
            
            // 4. Verify links are world-scoped
            GameObject linkedGO1 = world1.GetGameObject(entity1);
            GameObject linkedGO2 = world2.GetGameObject(entity2);
            
            AssertNotNull(linkedGO1, "World1 should have link");
            AssertNotNull(linkedGO2, "World2 should have link");
            AssertEquals(gameObject1, linkedGO1, "World1 link should match");
            AssertEquals(gameObject2, linkedGO2, "World2 link should match");
            
            // 5. Verify cross-world lookups return null
            GameObject crossWorldGO = world1.GetGameObject(entity2);
            AssertNull(crossWorldGO, "Cross-world lookup should return null");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject1);
            UnityEngine.Object.DestroyImmediate(gameObject2);
        });
    }
    
    [ContextMenu("Run Test: Multiple entities can be linked to different GameObjects")]
    public void Test_Link_021()
    {
        string testName = "Test_Link_021";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple GameObjects
            GameObject gameObject1 = new GameObject("TestObject1");
            GameObject gameObject2 = new GameObject("TestObject2");
            GameObject gameObject3 = new GameObject("TestObject3");
            
            // 2. Create entities linked to different GameObjects
            Entity entity1 = World.CreateEntity(gameObject1);
            Entity entity2 = World.CreateEntity(gameObject2);
            Entity entity3 = World.CreateEntity(gameObject3);
            
            // 3. Verify all links work
            GameObject linkedGO1 = World.GetGameObject(entity1);
            GameObject linkedGO2 = World.GetGameObject(entity2);
            GameObject linkedGO3 = World.GetGameObject(entity3);
            
            AssertEquals(gameObject1, linkedGO1, "Entity1 should be linked to GameObject1");
            AssertEquals(gameObject2, linkedGO2, "Entity2 should be linked to GameObject2");
            AssertEquals(gameObject3, linkedGO3, "Entity3 should be linked to GameObject3");
            
            // 4. Verify reverse lookups work
            Entity? retrievedEntity1 = World.GetEntity(gameObject1);
            Entity? retrievedEntity2 = World.GetEntity(gameObject2);
            Entity? retrievedEntity3 = World.GetEntity(gameObject3);
            
            AssertEquals(entity1, retrievedEntity1.Value, "Reverse lookup should work for entity1");
            AssertEquals(entity2, retrievedEntity2.Value, "Reverse lookup should work for entity2");
            AssertEquals(entity3, retrievedEntity3.Value, "Reverse lookup should work for entity3");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject1);
            UnityEngine.Object.DestroyImmediate(gameObject2);
            UnityEngine.Object.DestroyImmediate(gameObject3);
        });
    }
    
    // ========== Additional Edge Case Tests ==========
    
    [ContextMenu("Run Test: UnlinkEntity is safe for unlinked entity")]
    public void Test_Link_022()
    {
        string testName = "Test_Link_022";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity without GameObject
            Entity entity = World.CreateEntity();
            
            // 2. Destroy entity (UnlinkEntity should be called but entity is not linked)
            bool destroyed = World.DestroyEntity(entity);
            Assert(destroyed, "Entity should be destroyed");
            
            // 3. Test passes if no exception is thrown
            Assert(true, "UnlinkEntity should be safe for unlinked entity");
        });
    }
    
    [ContextMenu("Run Test: GetGameObject cleans up destroyed GameObject link")]
    public void Test_Link_023()
    {
        string testName = "Test_Link_023";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Verify link exists
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            
            // 3. Destroy GameObject
            UnityEngine.Object.DestroyImmediate(gameObject);
            
            // 4. GetGameObject should detect destroyed GameObject and clean up
            GameObject retrievedGO = World.GetGameObject(entity);
            AssertNull(retrievedGO, "GetGameObject should return null and clean up destroyed GameObject link");
            
            // 5. Verify reverse lookup also returns null after cleanup
            Entity? retrievedEntity = World.GetEntity(gameObject);
            Assert(!retrievedEntity.HasValue, "GetEntity should return null after GameObject destruction and cleanup");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: Entity can be relinked to different GameObject after unlinking")]
    public void Test_Link_024()
    {
        string testName = "Test_Link_024";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity and first GameObject
            GameObject gameObject1 = new GameObject("TestObject1");
            Entity entity = World.CreateEntity(gameObject1);
            
            // 2. Verify first link
            GameObject linkedGO1 = World.GetGameObject(entity);
            AssertEquals(gameObject1, linkedGO1, "First GameObject should be linked");
            
            // 3. Destroy entity to unlink (GameObject1 is destroyed by DestroyEntity)
            World.DestroyEntity(entity);
            
            // 4. Create new entity (may reuse same ID)
            Entity newEntity = World.CreateEntity();
            
            // 5. Create second GameObject and link to new entity
            GameObject gameObject2 = new GameObject("TestObject2");
            Entity linkedEntity = World.CreateEntity(gameObject2);
            
            // 6. Verify second GameObject is linked to new entity
            GameObject linkedGO2 = World.GetGameObject(linkedEntity);
            AssertEquals(gameObject2, linkedGO2, "Second GameObject should be linked to new entity");
            
            // Note: gameObject1 is already destroyed by DestroyEntity, so GetEntity will return null
            // (GetEntity checks if gameObject is null and returns null)
            
            // Cleanup
            // Note: gameObject1 is already destroyed by DestroyEntity
            UnityEngine.Object.DestroyImmediate(gameObject2);
        });
    }
    
    [ContextMenu("Run Test: GetEntity returns correct entity for linked GameObject")]
    public void Test_Link_025()
    {
        string testName = "Test_Link_025";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple GameObjects
            GameObject gameObject1 = new GameObject("TestObject1");
            GameObject gameObject2 = new GameObject("TestObject2");
            
            // 2. Create entities linked to GameObjects
            Entity entity1 = World.CreateEntity(gameObject1);
            Entity entity2 = World.CreateEntity(gameObject2);
            
            // 3. Verify GetEntity returns correct entity for each GameObject
            Entity? retrievedEntity1 = World.GetEntity(gameObject1);
            Entity? retrievedEntity2 = World.GetEntity(gameObject2);
            
            Assert(retrievedEntity1.HasValue, "GetEntity should return entity for gameObject1");
            Assert(retrievedEntity2.HasValue, "GetEntity should return entity for gameObject2");
            AssertEquals(entity1, retrievedEntity1.Value, "GetEntity should return correct entity for gameObject1");
            AssertEquals(entity2, retrievedEntity2.Value, "GetEntity should return correct entity for gameObject2");
            Assert(entity1 != entity2, "Entities should be different");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject1);
            UnityEngine.Object.DestroyImmediate(gameObject2);
        });
    }
    
    [ContextMenu("Run Test: Dictionary synchronization after multiple link/unlink operations")]
    public void Test_Link_026()
    {
        string testName = "Test_Link_026";
        ExecuteTest(testName, () =>
        {
            // 1. Create first GameObject and entity
            GameObject gameObject1 = new GameObject("TestObject1");
            Entity entity1 = World.CreateEntity(gameObject1);
            World.DestroyEntity(entity1);
            
            // 2. Create second GameObject and entity (gameObject1 was destroyed, so we need a new one)
            GameObject gameObject2 = new GameObject("TestObject2");
            Entity entity2 = World.CreateEntity(gameObject2);
            World.DestroyEntity(entity2);
            
            // 3. Create third GameObject and entity
            GameObject gameObject3 = new GameObject("TestObject3");
            Entity entity3 = World.CreateEntity(gameObject3);
            
            // 4. Verify link exists and is correct
            GameObject linkedGO = World.GetGameObject(entity3);
            AssertNotNull(linkedGO, "GameObject should be linked");
            AssertEquals(gameObject3, linkedGO, "Linked GameObject should match");
            
            // 5. Verify reverse lookup works
            Entity? retrievedEntity = World.GetEntity(gameObject3);
            Assert(retrievedEntity.HasValue, "GetEntity should return entity");
            AssertEquals(entity3, retrievedEntity.Value, "GetEntity should return correct entity");
            
            // Cleanup
            // Note: gameObject1 and gameObject2 are already destroyed by DestroyEntity
            UnityEngine.Object.DestroyImmediate(gameObject3);
        });
    }
    
    [ContextMenu("Run Test: GetGameObject handles null GameObject in dictionary")]
    public void Test_Link_027()
    {
        string testName = "Test_Link_027";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Verify link exists
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            
            // 3. Destroy GameObject (but don't unlink entity)
            UnityEngine.Object.DestroyImmediate(gameObject);
            
            // 4. GetGameObject should detect null and clean up
            GameObject retrievedGO = World.GetGameObject(entity);
            AssertNull(retrievedGO, "GetGameObject should return null for destroyed GameObject and clean up link");
            
            // 5. Verify link is removed from both dictionaries
            Entity? retrievedEntity = World.GetEntity(gameObject);
            Assert(!retrievedEntity.HasValue, "GetEntity should return null after cleanup");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: CreateEntity with GameObject via WorldInstance throws exception for null")]
    public void Test_Link_028()
    {
        string testName = "Test_Link_028";
        ExecuteTest(testName, () =>
        {
            // 1. Get world instance
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Try to create entity with null GameObject
            bool exceptionThrown = false;
            try
            {
                world.CreateEntity(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            
            // 3. Verify exception was thrown
            Assert(exceptionThrown, "ArgumentNullException should be thrown for null GameObject via WorldInstance");
        });
    }
    
    [ContextMenu("Run Test: GetGameObject returns null for invalid entity")]
    public void Test_Link_029()
    {
        string testName = "Test_Link_029";
        ExecuteTest(testName, () =>
        {
            // 1. Create invalid entity
            Entity invalidEntity = Entity.Invalid;
            
            // 2. Get GameObject for invalid entity
            GameObject retrievedGO = World.GetGameObject(invalidEntity);
            
            // 3. Verify null is returned
            AssertNull(retrievedGO, "GetGameObject should return null for invalid entity");
        });
    }
    
    [ContextMenu("Run Test: Multiple worlds maintain separate GameObject links")]
    public void Test_Link_030()
    {
        string testName = "Test_Link_030";
        ExecuteTest(testName, () =>
        {
            // 1. Create two world instances
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create same GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 3. Create entities in different worlds with same GameObject
            Entity entity1 = world1.CreateEntity(gameObject);
            Entity entity2 = world2.CreateEntity(gameObject);
            
            // 4. Verify both worlds have links
            GameObject linkedGO1 = world1.GetGameObject(entity1);
            GameObject linkedGO2 = world2.GetGameObject(entity2);
            
            AssertNotNull(linkedGO1, "World1 should have link");
            AssertNotNull(linkedGO2, "World2 should have link");
            AssertEquals(gameObject, linkedGO1, "World1 link should match");
            AssertEquals(gameObject, linkedGO2, "World2 link should match");
            
            // 5. Verify reverse lookups work in each world
            Entity? retrievedEntity1 = world1.GetEntity(gameObject);
            Entity? retrievedEntity2 = world2.GetEntity(gameObject);
            
            Assert(retrievedEntity1.HasValue, "World1 reverse lookup should work");
            Assert(retrievedEntity2.HasValue, "World2 reverse lookup should work");
            AssertEquals(entity1, retrievedEntity1.Value, "World1 should return correct entity");
            AssertEquals(entity2, retrievedEntity2.Value, "World2 should return correct entity");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity unlinks GameObject even if entity has no components")]
    public void Test_Link_031()
    {
        string testName = "Test_Link_031";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity without components
            GameObject gameObject = new GameObject("TestObject");
            Entity entity = World.CreateEntity(gameObject);
            
            // 2. Verify link exists
            GameObject linkedGO = World.GetGameObject(entity);
            AssertNotNull(linkedGO, "GameObject should be linked");
            
            // 3. Destroy entity (no components to remove, GameObject is destroyed by DestroyEntity)
            bool destroyed = World.DestroyEntity(entity);
            Assert(destroyed, "Entity should be destroyed");
            
            // 4. Verify link is removed
            GameObject retrievedGO = World.GetGameObject(entity);
            AssertNull(retrievedGO, "GetGameObject should return null after entity destruction");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
    
    [ContextMenu("Run Test: GetGameObject and GetEntity work correctly after entity pool reuse")]
    public void Test_Link_032()
    {
        string testName = "Test_Link_032";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject and entity
            GameObject gameObject1 = new GameObject("TestObject1");
            Entity entity1 = World.CreateEntity(gameObject1);
            int entity1Id = entity1.Id;
            
            // 2. Destroy entity (gameObject1 is destroyed by DestroyEntity)
            World.DestroyEntity(entity1);
            
            // 3. Create new entity (may reuse same ID from pool)
            GameObject gameObject2 = new GameObject("TestObject2");
            Entity entity2 = World.CreateEntity(gameObject2);
            
            // 4. Verify new entity is linked correctly
            GameObject linkedGO2 = World.GetGameObject(entity2);
            AssertNotNull(linkedGO2, "New entity should be linked");
            AssertEquals(gameObject2, linkedGO2, "New entity should be linked to gameObject2");
            
            // 5. Verify old GameObject is not linked (gameObject1 is destroyed, so GetEntity returns null)
            // Note: gameObject1 is already destroyed by DestroyEntity, so GetEntity will return null
            // (GetEntity checks if gameObject is null and returns null)
            
            // 6. Verify new GameObject is linked
            Entity? retrievedEntity2 = World.GetEntity(gameObject2);
            Assert(retrievedEntity2.HasValue, "New GameObject should be linked");
            AssertEquals(entity2, retrievedEntity2.Value, "New GameObject should be linked to new entity");
            
            // Cleanup
            // Note: gameObject1 is already destroyed by DestroyEntity
            UnityEngine.Object.DestroyImmediate(gameObject2);
        });
    }
    
    [ContextMenu("Run Test: World static methods delegate correctly to GlobalWorld")]
    public void Test_Link_033()
    {
        string testName = "Test_Link_033";
        ExecuteTest(testName, () =>
        {
            // 1. Create GameObject
            GameObject gameObject = new GameObject("TestObject");
            
            // 2. Create entity via World static method
            Entity entity = World.CreateEntity(gameObject);
            
            // 3. Get GameObject via World static method
            GameObject retrievedGO = World.GetGameObject(entity);
            AssertNotNull(retrievedGO, "World.GetGameObject should return GameObject");
            AssertEquals(gameObject, retrievedGO, "World.GetGameObject should return correct GameObject");
            
            // 4. Get entity via World static method
            Entity? retrievedEntity = World.GetEntity(gameObject);
            Assert(retrievedEntity.HasValue, "World.GetEntity should return entity");
            AssertEquals(entity, retrievedEntity.Value, "World.GetEntity should return correct entity");
            
            // 5. Verify GlobalWorld methods return same results
            GameObject globalGO = World.GlobalWorld.GetGameObject(entity);
            Entity? globalEntity = World.GlobalWorld.GetEntity(gameObject);
            
            AssertEquals(retrievedGO, globalGO, "World.GetGameObject should match GlobalWorld.GetGameObject");
            AssertEquals(retrievedEntity.Value, globalEntity.Value, "World.GetEntity should match GlobalWorld.GetEntity");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(gameObject);
        });
    }
}