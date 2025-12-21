#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [InitializeOnLoad]
    public class EcsHierarchyManager : MonoBehaviour
    {
        private static EcsHierarchyManager _instance;
        private static bool _initialized = false;

        public static EcsHierarchyManager Instance => _instance;

        private GameObject _rootGameObject;
        private readonly Dictionary<WorldInstance, GameObject> _worldGameObjects = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _entitiesContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _systemsContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _updateContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _fixedUpdateContainers = new Dictionary<WorldInstance, GameObject>();
        
        private struct EntityWorldKey : IEquatable<EntityWorldKey>
        {
            public readonly Entity Entity;
            public readonly WorldInstance World;

            public EntityWorldKey(Entity entity, WorldInstance world)
            {
                Entity = entity;
                World = world;
            }

            public bool Equals(EntityWorldKey other)
            {
                return Entity.Equals(other.Entity) && ReferenceEquals(World, other.World);
            }

            public override int GetHashCode()
            {
                return Entity.GetHashCode() ^ World.GetHashCode();
            }
        }

        private readonly Dictionary<EntityWorldKey, GameObject> _entityGameObjects = new Dictionary<EntityWorldKey, GameObject>();
        
        private struct SystemKey : IEquatable<SystemKey>
        {
            public readonly SystemHandler System;
            public readonly WorldInstance World;
            public readonly string QueueName;

            public SystemKey(SystemHandler system, WorldInstance world, string queueName)
            {
                System = system;
                World = world;
                QueueName = queueName;
            }

            public bool Equals(SystemKey other)
            {
                return ReferenceEquals(System, other.System) && ReferenceEquals(World, other.World) && QueueName == other.QueueName;
            }

            public override int GetHashCode()
            {
                return System.GetHashCode() ^ World.GetHashCode() ^ QueueName.GetHashCode();
            }
        }

        private readonly Dictionary<SystemKey, GameObject> _systemGameObjects = new Dictionary<SystemKey, GameObject>();

        private float _lastUpdateTime;
        private const float UPDATE_INTERVAL = 0.5f;

        static EcsHierarchyManager()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EnsureCreated();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                CleanupInstance();
            }
        }

        private static void EnsureCreated()
        {
            if (_instance != null && _instance.gameObject != null)
            {
                return;
            }

            if (_instance != null && _instance.gameObject == null)
            {
                _instance = null;
            }

            GameObject root = GameObject.Find("ArtyEcs");
            if (root == null)
            {
                root = new GameObject("ArtyEcs");
            }

            _instance = root.GetComponent<EcsHierarchyManager>();
            if (_instance == null)
            {
                _instance = root.AddComponent<EcsHierarchyManager>();
            }
            _instance.Initialize();
        }

        private static void CleanupInstance()
        {
            if (_instance != null)
            {
                _instance.CleanupHierarchy();
                if (_instance.gameObject != null)
                {
                    DestroyImmediate(_instance.gameObject);
                }
                _instance = null;
            }
            _initialized = false;
        }

        private void Awake()
        {
            if (_initialized && _instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple EcsHierarchyManager instances detected. Destroying duplicate.");
                DestroyImmediate(gameObject);
                return;
            }

            _initialized = true;
            _instance = this;
        }

        private void Initialize()
        {
            GetOrCreateRoot();
            UpdateWorldGameObjects();
            
            var allWorlds = World.GetAllWorlds();
            foreach (var world in allWorlds)
            {
                UpdateEntityGameObjects(world);
                UpdateSystemGameObjects(world);
            }
            
            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastUpdateTime >= UPDATE_INTERVAL)
            {
                UpdateHierarchy();
                _lastUpdateTime = currentTime;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                CleanupHierarchy();
                _initialized = false;
                _instance = null;
            }
        }

        private GameObject GetOrCreateRoot()
        {
            if (_rootGameObject != null)
            {
                if (_rootGameObject.transform.parent == null)
                {
                    return _rootGameObject;
                }
            }

            GameObject existing = GameObject.Find("ArtyEcs");
            if (existing != null)
            {
                _rootGameObject = existing;
                _rootGameObject.transform.SetParent(null);
                return _rootGameObject;
            }

            _rootGameObject = gameObject;
            _rootGameObject.transform.SetParent(null);
            if (_rootGameObject.name != "ArtyEcs")
            {
                _rootGameObject.name = "ArtyEcs";
            }
            return _rootGameObject;
        }

        public GameObject GetOrCreateWorldGameObject(WorldInstance world)
        {
            if (world == null)
            {
                return null;
            }

            if (_worldGameObjects.TryGetValue(world, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _worldGameObjects.Remove(world);
            }

            var root = GetOrCreateRoot();
            string worldName = world.Name ?? "Unknown";
            var worldGO = new GameObject(worldName);
            worldGO.transform.SetParent(root.transform);
            _worldGameObjects[world] = worldGO;
            
            GetOrCreateEntitiesContainer(world);
            GetOrCreateSystemsContainer(world);
            GetOrCreateUpdateContainer(world);
            GetOrCreateFixedUpdateContainer(world);
            
            return worldGO;
        }

        private GameObject GetOrCreateEntitiesContainer(WorldInstance world)
        {
            if (world == null)
            {
                return null;
            }

            if (_entitiesContainers.TryGetValue(world, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _entitiesContainers.Remove(world);
            }

            var worldGO = GetOrCreateWorldGameObject(world);
            if (worldGO == null)
            {
                return null;
            }

            Transform entitiesTransform = worldGO.transform.Find("Entities");
            if (entitiesTransform != null)
            {
                _entitiesContainers[world] = entitiesTransform.gameObject;
                return entitiesTransform.gameObject;
            }

            var entitiesGO = new GameObject("Entities");
            entitiesGO.transform.SetParent(worldGO.transform);
            _entitiesContainers[world] = entitiesGO;
            return entitiesGO;
        }

        private GameObject GetOrCreateSystemsContainer(WorldInstance world)
        {
            if (world == null)
            {
                return null;
            }

            if (_systemsContainers.TryGetValue(world, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _systemsContainers.Remove(world);
            }

            var worldGO = GetOrCreateWorldGameObject(world);
            if (worldGO == null)
            {
                return null;
            }

            Transform systemsTransform = worldGO.transform.Find("Systems");
            if (systemsTransform != null)
            {
                _systemsContainers[world] = systemsTransform.gameObject;
                return systemsTransform.gameObject;
            }

            var systemsGO = new GameObject("Systems");
            systemsGO.transform.SetParent(worldGO.transform);
            _systemsContainers[world] = systemsGO;
            return systemsGO;
        }

        private GameObject GetOrCreateUpdateContainer(WorldInstance world)
        {
            if (world == null)
            {
                return null;
            }

            if (_updateContainers.TryGetValue(world, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _updateContainers.Remove(world);
            }

            var systemsGO = GetOrCreateSystemsContainer(world);
            if (systemsGO == null)
            {
                return null;
            }

            Transform updateTransform = systemsGO.transform.Find("Update");
            if (updateTransform != null)
            {
                _updateContainers[world] = updateTransform.gameObject;
                return updateTransform.gameObject;
            }

            var updateGO = new GameObject("Update");
            updateGO.transform.SetParent(systemsGO.transform);
            _updateContainers[world] = updateGO;
            return updateGO;
        }

        private GameObject GetOrCreateFixedUpdateContainer(WorldInstance world)
        {
            if (world == null)
            {
                return null;
            }

            if (_fixedUpdateContainers.TryGetValue(world, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _fixedUpdateContainers.Remove(world);
            }

            var systemsGO = GetOrCreateSystemsContainer(world);
            if (systemsGO == null)
            {
                return null;
            }

            Transform fixedUpdateTransform = systemsGO.transform.Find("FixedUpdate");
            if (fixedUpdateTransform != null)
            {
                _fixedUpdateContainers[world] = fixedUpdateTransform.gameObject;
                return fixedUpdateTransform.gameObject;
            }

            var fixedUpdateGO = new GameObject("FixedUpdate");
            fixedUpdateGO.transform.SetParent(systemsGO.transform);
            _fixedUpdateContainers[world] = fixedUpdateGO;
            return fixedUpdateGO;
        }

        public GameObject GetOrCreateEntityGameObject(Entity entity, WorldInstance world)
        {
            if (!entity.IsValid || world == null)
            {
                return null;
            }

            var key = new EntityWorldKey(entity, world);
            string expectedName = $"Entity_{entity.Id}_Gen{entity.Generation}";

            if (_entityGameObjects.TryGetValue(key, out var existing))
            {
                if (existing != null)
                {
                    if (existing.name == expectedName)
                    {
                        return existing;
                    }
                    
                    DestroyImmediate(existing);
                    _entityGameObjects.Remove(key);
                }
                else
                {
                    _entityGameObjects.Remove(key);
                }
            }

            var entitiesContainer = GetOrCreateEntitiesContainer(world);
            if (entitiesContainer == null)
            {
                return null;
            }

            var entityGO = new GameObject(expectedName);
            entityGO.transform.SetParent(entitiesContainer.transform);
            _entityGameObjects[key] = entityGO;
            return entityGO;
        }

        public GameObject GetOrCreateSystemGameObject(SystemHandler system, WorldInstance world, string queueName)
        {
            if (system == null || world == null || string.IsNullOrEmpty(queueName))
            {
                return null;
            }

            var key = new SystemKey(system, world, queueName);
            if (_systemGameObjects.TryGetValue(key, out var existing))
            {
                if (existing != null)
                {
                    return existing;
                }
                _systemGameObjects.Remove(key);
            }

            GameObject container = null;
            if (queueName == "Update")
            {
                container = GetOrCreateUpdateContainer(world);
            }
            else if (queueName == "FixedUpdate")
            {
                container = GetOrCreateFixedUpdateContainer(world);
            }
            else
            {
                return null;
            }

            if (container == null)
            {
                return null;
            }

            string baseSystemName = system.GetType().Name;
            string systemName = GenerateUniqueSystemName(baseSystemName, container, system, world, queueName);
            
            var systemGO = new GameObject(systemName);
            systemGO.transform.SetParent(container.transform);
            _systemGameObjects[key] = systemGO;
            return systemGO;
        }

        private string GenerateUniqueSystemName(string baseName, GameObject container, SystemHandler system, WorldInstance world, string queueName)
        {
            var currentKey = new SystemKey(system, world, queueName);
            int index = 0;
            string candidateName = baseName;
            
            while (true)
            {
                bool nameExists = false;
                for (int i = 0; i < container.transform.childCount; i++)
                {
                    Transform child = container.transform.GetChild(i);
                    if (child.name == candidateName)
                    {
                        var existingKey = FindSystemKeyByGameObject(child.gameObject);
                        if (!existingKey.HasValue || !existingKey.Value.Equals(currentKey))
                        {
                            nameExists = true;
                            break;
                        }
                    }
                }
                
                if (!nameExists)
                {
                    return candidateName;
                }
                
                index++;
                candidateName = $"{baseName}_{index}";
            }
        }

        private SystemKey? FindSystemKeyByGameObject(GameObject gameObject)
        {
            foreach (var kvp in _systemGameObjects)
            {
                if (kvp.Value == gameObject)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public void CleanupHierarchy()
        {
            foreach (var kvp in _worldGameObjects)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            _worldGameObjects.Clear();
            _entitiesContainers.Clear();
            _systemsContainers.Clear();
            _updateContainers.Clear();
            _fixedUpdateContainers.Clear();
            _entityGameObjects.Clear();
            _systemGameObjects.Clear();
            _rootGameObject = null;
        }

        private void UpdateWorldGameObjects()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var allWorlds = World.GetAllWorlds();
            HashSet<WorldInstance> currentWorlds = new HashSet<WorldInstance>();

            foreach (var world in allWorlds)
            {
                currentWorlds.Add(world);
                GetOrCreateWorldGameObject(world);
            }

            var worldsToRemove = new List<WorldInstance>();
            foreach (var kvp in _worldGameObjects)
            {
                if (!currentWorlds.Contains(kvp.Key) || kvp.Value == null)
                {
                    if (kvp.Value != null)
                    {
                        DestroyImmediate(kvp.Value);
                    }
                    worldsToRemove.Add(kvp.Key);
                }
            }
            foreach (var world in worldsToRemove)
            {
                _worldGameObjects.Remove(world);
            }
        }

        private void UpdateEntityGameObjects(WorldInstance world)
        {
            if (world == null || !Application.isPlaying)
            {
                return;
            }

            var entities = world.GetAllEntities();
            HashSet<Entity> currentEntities = new HashSet<Entity>();

            foreach (var entity in entities)
            {
                currentEntities.Add(entity);
                GetOrCreateEntityGameObject(entity, world);
            }

            var entitiesToRemove = new List<EntityWorldKey>();
            foreach (var kvp in _entityGameObjects)
            {
                if (ReferenceEquals(kvp.Key.World, world))
                {
                    if (!currentEntities.Contains(kvp.Key.Entity) || kvp.Value == null)
                    {
                        if (kvp.Value != null)
                        {
                            DestroyImmediate(kvp.Value);
                        }
                        entitiesToRemove.Add(kvp.Key);
                    }
                }
            }
            foreach (var key in entitiesToRemove)
            {
                _entityGameObjects.Remove(key);
            }
        }

        public void UpdateHierarchy()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateWorldGameObjects();

            var root = GetOrCreateRoot();
            var allWorlds = World.GetAllWorlds();

            foreach (var world in allWorlds)
            {
                UpdateEntityGameObjects(world);
                UpdateSystemGameObjects(world);
            }
        }

        private void UpdateSystemGameObjects(WorldInstance world)
        {
            if (world == null || !Application.isPlaying)
            {
                return;
            }

            var updateQueue = world.GetUpdateQueue();
            var fixedUpdateQueue = world.GetFixedUpdateQueue();

            HashSet<SystemKey> currentSystems = new HashSet<SystemKey>();

            GameObject updateContainer = GetOrCreateUpdateContainer(world);
            GameObject fixedUpdateContainer = GetOrCreateFixedUpdateContainer(world);

            for (int i = 0; i < updateQueue.Count; i++)
            {
                var system = updateQueue[i];
                var key = new SystemKey(system, world, "Update");
                currentSystems.Add(key);
                
                var systemGO = GetOrCreateSystemGameObject(system, world, "Update");
                if (systemGO != null && systemGO.transform.parent == updateContainer.transform)
                {
                    systemGO.transform.SetSiblingIndex(i);
                }
            }

            for (int i = 0; i < fixedUpdateQueue.Count; i++)
            {
                var system = fixedUpdateQueue[i];
                var key = new SystemKey(system, world, "FixedUpdate");
                currentSystems.Add(key);
                
                var systemGO = GetOrCreateSystemGameObject(system, world, "FixedUpdate");
                if (systemGO != null && systemGO.transform.parent == fixedUpdateContainer.transform)
                {
                    systemGO.transform.SetSiblingIndex(i);
                }
            }

            var systemsToRemove = new List<SystemKey>();
            foreach (var kvp in _systemGameObjects)
            {
                if (ReferenceEquals(kvp.Key.World, world))
                {
                    if (!currentSystems.Contains(kvp.Key) || kvp.Value == null)
                    {
                        if (kvp.Value != null)
                        {
                            DestroyImmediate(kvp.Value);
                        }
                        systemsToRemove.Add(kvp.Key);
                    }
                }
            }
            foreach (var systemKey in systemsToRemove)
            {
                _systemGameObjects.Remove(systemKey);
            }
        }
    }
}
#endif

