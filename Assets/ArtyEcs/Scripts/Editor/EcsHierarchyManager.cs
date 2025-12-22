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
        #region Constants
        private const float UPDATE_INTERVAL = 0.5f;
        private const string CONTAINER_ENTITIES = "Entities";
        private const string CONTAINER_SYSTEMS = "Systems";
        private const string CONTAINER_UPDATE = "Update";
        private const string CONTAINER_FIXED_UPDATE = "FixedUpdate";
        private const string QUEUE_UPDATE = "Update";
        private const string QUEUE_FIXED_UPDATE = "FixedUpdate";
        #endregion

        #region Static Fields and Properties
        private static EcsHierarchyManager _instance;
        private static bool _initialized = false;

        public static EcsHierarchyManager Instance => _instance;
        #endregion

        #region Instance Fields
        private GameObject _rootGameObject;
        private readonly Dictionary<WorldInstance, GameObject> _worldGameObjects = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _entitiesContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _systemsContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _updateContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<WorldInstance, GameObject> _fixedUpdateContainers = new Dictionary<WorldInstance, GameObject>();
        private readonly Dictionary<EntityWorldKey, GameObject> _entityGameObjects = new Dictionary<EntityWorldKey, GameObject>();
        private readonly Dictionary<WorldInstance, HashSet<Entity>> _previousEntitySets = new Dictionary<WorldInstance, HashSet<Entity>>();
        private readonly Dictionary<SystemKey, GameObject> _systemGameObjects = new Dictionary<SystemKey, GameObject>();
        private readonly Dictionary<WorldQueueKey, List<SystemHandler>> _previousSystemLists = new Dictionary<WorldQueueKey, List<SystemHandler>>();

        private float _lastUpdateTime;
        
        [SerializeField]
        private bool _preserveOnExit = false;
        #endregion

        #region Static Initialization
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
        #endregion

        #region Unity Lifecycle
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

        private void OnDisable()
        {
            if (!Application.isPlaying && _instance == this)
            {
                CleanupHierarchy();
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
        #endregion

        #region Initialization
        private void Initialize()
        {
            GetOrCreateRoot();
            UpdateWorldGameObjects();
            
            var allWorlds = World.GetAllWorlds();
            foreach (var world in allWorlds)
            {
                var entities = world.GetAllEntities();
                HashSet<Entity> initialEntitySet = new HashSet<Entity>();
                foreach (var entity in entities)
                {
                    initialEntitySet.Add(entity);
                    GetOrCreateEntityGameObject(entity, world);
                }
                _previousEntitySets[world] = initialEntitySet;
                
                UpdateSystemHierarchy(world);
            }
            
            _lastUpdateTime = Time.realtimeSinceStartup;
        }
        #endregion

        #region Helper Methods
        private static bool TryGetValidValue<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, out TValue value) where TValue : class
        {
            if (dict.TryGetValue(key, out value))
            {
                if (value != null)
                {
                    return true;
                }
                dict.Remove(key);
            }
            value = null;
            return false;
        }

        private GameObject GetOrCreateContainer(WorldInstance world, Dictionary<WorldInstance, GameObject> containerDict, string containerName, GameObject parent)
        {
            if (world == null)
            {
                return null;
            }

            if (TryGetValidValue(containerDict, world, out var existing))
            {
                return existing;
            }

            if (parent == null)
            {
                return null;
            }

            Transform containerTransform = parent.transform.Find(containerName);
            if (containerTransform != null)
            {
                containerDict[world] = containerTransform.gameObject;
                return containerTransform.gameObject;
            }

            var containerGO = new GameObject(containerName);
            containerGO.transform.SetParent(parent.transform);
            containerDict[world] = containerGO;
            return containerGO;
        }

        private void CleanupGameObjectDictionary<TKey>(Dictionary<TKey, GameObject> dict)
        {
            var keysToCleanup = new List<TKey>(dict.Keys);
            foreach (var key in keysToCleanup)
            {
                if (dict.TryGetValue(key, out var gameObject) && gameObject != null)
                {
                    DestroyImmediate(gameObject);
                }
            }
            dict.Clear();
        }
        #endregion

        #region Root/World GameObject Management
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

            if (TryGetValidValue(_worldGameObjects, world, out var existing))
            {
                return existing;
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
        #endregion

        #region Container GameObject Management
        private GameObject GetOrCreateEntitiesContainer(WorldInstance world)
        {
            var worldGO = GetOrCreateWorldGameObject(world);
            return GetOrCreateContainer(world, _entitiesContainers, CONTAINER_ENTITIES, worldGO);
        }

        private GameObject GetOrCreateSystemsContainer(WorldInstance world)
        {
            var worldGO = GetOrCreateWorldGameObject(world);
            return GetOrCreateContainer(world, _systemsContainers, CONTAINER_SYSTEMS, worldGO);
        }

        private GameObject GetOrCreateUpdateContainer(WorldInstance world)
        {
            var systemsGO = GetOrCreateSystemsContainer(world);
            return GetOrCreateContainer(world, _updateContainers, CONTAINER_UPDATE, systemsGO);
        }

        private GameObject GetOrCreateFixedUpdateContainer(WorldInstance world)
        {
            var systemsGO = GetOrCreateSystemsContainer(world);
            return GetOrCreateContainer(world, _fixedUpdateContainers, CONTAINER_FIXED_UPDATE, systemsGO);
        }
        #endregion

        #region Entity GameObject Management
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
                        var display = existing.GetComponent<EntityComponentDisplay>();
                        if (display == null)
                        {
                            display = existing.AddComponent<EntityComponentDisplay>();
                            display.Initialize(entity, world);
                        }
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
            
            var componentDisplay = entityGO.AddComponent<EntityComponentDisplay>();
            componentDisplay.Initialize(entity, world);
            
            _entityGameObjects[key] = entityGO;
            return entityGO;
        }

        private EntityWorldKey? FindEntityKeyByGameObject(GameObject gameObject, WorldInstance world)
        {
            foreach (var kvp in _entityGameObjects)
            {
                if (ReferenceEquals(kvp.Key.World, world) && kvp.Value == gameObject)
                {
                    return kvp.Key;
                }
            }
            return null;
        }
        #endregion

        #region System GameObject Management
        public GameObject GetOrCreateSystemGameObject(SystemHandler system, WorldInstance world, string queueName)
        {
            if (system == null || world == null || string.IsNullOrEmpty(queueName))
            {
                return null;
            }

            var key = new SystemKey(system, world, queueName);
            if (TryGetValidValue(_systemGameObjects, key, out var existing))
            {
                return existing;
            }

            GameObject container = null;
            if (queueName == QUEUE_UPDATE)
            {
                container = GetOrCreateUpdateContainer(world);
            }
            else if (queueName == QUEUE_FIXED_UPDATE)
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
        #endregion

        #region Update/Refresh Logic
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
                UpdateEntityHierarchy(world);
                UpdateSystemHierarchy(world);
            }
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
                _previousEntitySets.Remove(world);
            }
        }

        private void UpdateEntityHierarchy(WorldInstance world)
        {
            if (world == null || !Application.isPlaying)
            {
                return;
            }

            var currentEntitiesSpan = world.GetAllEntities();
            HashSet<Entity> currentEntities = new HashSet<Entity>();
            
            foreach (var entity in currentEntitiesSpan)
            {
                currentEntities.Add(entity);
            }

            if (!_previousEntitySets.TryGetValue(world, out var previousEntities))
            {
                previousEntities = new HashSet<Entity>();
                _previousEntitySets[world] = previousEntities;
            }

            var entitiesToAdd = new List<Entity>();
            var entitiesToRemove = new List<Entity>();

            foreach (var entity in currentEntities)
            {
                if (!previousEntities.Contains(entity))
                {
                    entitiesToAdd.Add(entity);
                }
            }

            foreach (var entity in previousEntities)
            {
                if (!currentEntities.Contains(entity))
                {
                    entitiesToRemove.Add(entity);
                }
            }

            foreach (var entity in entitiesToAdd)
            {
                GetOrCreateEntityGameObject(entity, world);
            }

            foreach (var entity in entitiesToRemove)
            {
                var key = new EntityWorldKey(entity, world);
                if (_entityGameObjects.TryGetValue(key, out var gameObject))
                {
                    if (gameObject != null)
                    {
                        DestroyImmediate(gameObject);
                    }
                    _entityGameObjects.Remove(key);
                }
            }

            var entitiesContainer = GetOrCreateEntitiesContainer(world);
            if (entitiesContainer != null)
            {
                var orphanedGameObjects = new List<GameObject>();
                for (int i = 0; i < entitiesContainer.transform.childCount; i++)
                {
                    var child = entitiesContainer.transform.GetChild(i).gameObject;
                    var key = FindEntityKeyByGameObject(child, world);
                    if (!key.HasValue || !currentEntities.Contains(key.Value.Entity))
                    {
                        orphanedGameObjects.Add(child);
                    }
                }
                foreach (var orphaned in orphanedGameObjects)
                {
                    DestroyImmediate(orphaned);
                }
            }

            _previousEntitySets[world] = new HashSet<Entity>(currentEntities);
        }

        private void UpdateSystemHierarchy(WorldInstance world)
        {
            if (world == null || !Application.isPlaying)
            {
                return;
            }

            var updateQueue = world.GetUpdateQueue();
            var fixedUpdateQueue = world.GetFixedUpdateQueue();

            UpdateSystemQueue(world, QUEUE_UPDATE, updateQueue);
            UpdateSystemQueue(world, QUEUE_FIXED_UPDATE, fixedUpdateQueue);
        }

        private void UpdateSystemQueue(WorldInstance world, string queueName, IReadOnlyList<SystemHandler> currentQueue)
        {
            var queueKey = new WorldQueueKey(world, queueName);
            
            List<SystemHandler> previousList = null;
            if (!_previousSystemLists.TryGetValue(queueKey, out previousList))
            {
                previousList = new List<SystemHandler>();
                _previousSystemLists[queueKey] = previousList;
            }

            List<SystemHandler> currentList = new List<SystemHandler>();
            for (int i = 0; i < currentQueue.Count; i++)
            {
                currentList.Add(currentQueue[i]);
            }

            bool hasChanges = false;
            if (previousList.Count != currentList.Count)
            {
                hasChanges = true;
            }
            else
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    if (!ReferenceEquals(previousList[i], currentList[i]))
                    {
                        hasChanges = true;
                        break;
                    }
                }
            }

            if (!hasChanges)
            {
                bool missingGameObjects = false;
                foreach (var system in currentList)
                {
                    var key = new SystemKey(system, world, queueName);
                    if (!_systemGameObjects.TryGetValue(key, out var gameObject) || gameObject == null)
                    {
                        missingGameObjects = true;
                        break;
                    }
                }
                
                if (!missingGameObjects)
                {
                    return;
                }
            }

            GameObject container = null;
            if (queueName == QUEUE_UPDATE)
            {
                container = GetOrCreateUpdateContainer(world);
            }
            else if (queueName == QUEUE_FIXED_UPDATE)
            {
                container = GetOrCreateFixedUpdateContainer(world);
            }
            else
            {
                return;
            }

            if (container == null)
            {
                return;
            }

            HashSet<SystemHandler> previousSet = new HashSet<SystemHandler>(previousList);
            HashSet<SystemHandler> currentSet = new HashSet<SystemHandler>(currentList);

            var systemsToAdd = new List<SystemHandler>();
            var systemsToRemove = new List<SystemHandler>();

            foreach (var system in currentList)
            {
                if (!previousSet.Contains(system))
                {
                    systemsToAdd.Add(system);
                }
            }

            foreach (var system in previousList)
            {
                if (!currentSet.Contains(system))
                {
                    systemsToRemove.Add(system);
                }
            }

            foreach (var system in systemsToRemove)
            {
                var key = new SystemKey(system, world, queueName);
                if (_systemGameObjects.TryGetValue(key, out var gameObject))
                {
                    if (gameObject != null)
                    {
                        DestroyImmediate(gameObject);
                    }
                    _systemGameObjects.Remove(key);
                }
            }

            for (int i = 0; i < currentList.Count; i++)
            {
                var system = currentList[i];
                var systemGO = GetOrCreateSystemGameObject(system, world, queueName);
                if (systemGO != null && systemGO.transform.parent == container.transform)
                {
                    systemGO.transform.SetSiblingIndex(i);
                }
            }

            _previousSystemLists[queueKey] = new List<SystemHandler>(currentList);
        }
        #endregion

        #region Cleanup Logic
        public void CleanupHierarchy()
        {
            if (_preserveOnExit)
            {
                return;
            }

            if (_rootGameObject != null && _rootGameObject != gameObject)
            {
                DestroyImmediate(_rootGameObject);
                _rootGameObject = null;
            }

            CleanupGameObjectDictionary(_worldGameObjects);
            CleanupGameObjectDictionary(_entityGameObjects);
            CleanupGameObjectDictionary(_systemGameObjects);

            _entitiesContainers.Clear();
            _systemsContainers.Clear();
            _updateContainers.Clear();
            _fixedUpdateContainers.Clear();
            _previousEntitySets.Clear();
            _previousSystemLists.Clear();
            
            if (_rootGameObject != gameObject)
            {
                _rootGameObject = null;
            }
        }
        #endregion
    }
}
#endif
