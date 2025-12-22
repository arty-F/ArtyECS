#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;
using ArtyECS.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(RuntimeTestScenario))]
public class RuntimeTestScenarioEditor : Editor
{
    private RuntimeTestScenario scenario;
    private int selectedComponentIndex = 0;
    private int selectedSystemIndex = 0;
    private bool showComponentPopup = false;
    private bool showSystemPopup = false;
    
    private string[] componentNames = new string[]
    {
        "TestComponent",
        "Position",
        "Velocity",
        "Health",
        "Dead",
        "Destroyed",
        "CounterComponent",
        "UpdateCounter",
        "FixedUpdateCounter",
        "Spawner",
        "Acceleration"
    };
    
    private string[] systemNames = new string[]
    {
        "IncrementSystem",
        "MovementSystem",
        "HealthSystem",
        "ModifiableHealthSystem",
        "PhysicsSystem",
        "SetValueSystem",
        "UpdateCounterSystem",
        "FixedUpdateCounterSystem",
        "CleanupSystem",
        "SpawnSystem"
    };
    
    private Dictionary<string, object> componentInputs = new Dictionary<string, object>();
    private int setValueSystemInput = 0;
    
    private void OnEnable()
    {
        scenario = (RuntimeTestScenario)target;
        InitializeComponentInputs();
    }
    
    private void InitializeComponentInputs()
    {
        componentInputs["TestComponent"] = 0;
        componentInputs["Position"] = Vector3.zero;
        componentInputs["Velocity"] = Vector3.zero;
        componentInputs["Health"] = 100f;
        componentInputs["CounterComponent"] = 0;
        componentInputs["UpdateCounter"] = 0;
        componentInputs["FixedUpdateCounter"] = 0;
        componentInputs["Spawner"] = 0;
        componentInputs["Acceleration"] = Vector3.zero;
    }
    
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This editor only works in Play Mode", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.LabelField("Runtime ECS Test Scenario", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        DrawWorldManagement();
        EditorGUILayout.Space();
        DrawEntityManagement();
        EditorGUILayout.Space();
        DrawSystemManagement();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(scenario);
        }
    }
    
    private void DrawWorldManagement()
    {
        EditorGUILayout.LabelField("World Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("World Name:", GUILayout.Width(100));
        string newWorldName = EditorGUILayout.TextField(scenario.WorldName);
        if (newWorldName != scenario.WorldName)
        {
            scenario.SetWorldName(newWorldName);
        }
        
        if (GUILayout.Button("Create", GUILayout.Width(80)))
        {
            if (scenario.CreateWorld(newWorldName))
            {
                Debug.Log($"World '{newWorldName}' created successfully");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create world '{newWorldName}'. It may already exist.", "OK");
            }
        }
        
        bool canDelete = !string.IsNullOrEmpty(newWorldName) && newWorldName != "Global" && World.Exists(newWorldName);
        EditorGUI.BeginDisabledGroup(!canDelete);
        if (GUILayout.Button("Delete", GUILayout.Width(80)))
        {
            if (scenario.DeleteWorld(newWorldName))
            {
                Debug.Log($"World '{newWorldName}' deleted successfully");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Failed to delete world '{newWorldName}'.", "OK");
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        var allWorlds = World.GetAllWorlds();
        if (allWorlds.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Existing Worlds:", GUILayout.Width(100));
            string[] worldNames = allWorlds.Select(w => w.Name).ToArray();
            int currentIndex = Array.IndexOf(worldNames, scenario.WorldName);
            if (currentIndex < 0) currentIndex = 0;
            int selectedIndex = EditorGUILayout.Popup(currentIndex, worldNames);
            if (selectedIndex >= 0 && selectedIndex < worldNames.Length && worldNames[selectedIndex] != scenario.WorldName)
            {
                scenario.SetWorldName(worldNames[selectedIndex]);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (World.Exists(scenario.WorldName))
        {
            var world = scenario.GetWorld();
            int entityCount = world.GetAllEntities().Length;
            int updateCount = world.GetUpdateQueue().Count;
            int fixedUpdateCount = world.GetFixedUpdateQueue().Count;
            
            EditorGUILayout.LabelField($"Status: World exists | Entities: {entityCount} | Systems: {updateCount + fixedUpdateCount} (Update: {updateCount}, FixedUpdate: {fixedUpdateCount})");
        }
        else
        {
            EditorGUILayout.LabelField("Status: World does not exist");
        }
    }
    
    private void DrawEntityManagement()
    {
        EditorGUILayout.LabelField("Entity Management", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Entity", GUILayout.Height(25)))
        {
            var world = scenario.GetWorld();
            if (world == null)
            {
                return;
            }
            
            var entity = scenario.CreateEntity(scenario.WorldName);
            Debug.Log($"Created entity: {scenario.EntityName}");
            
            if (EcsHierarchyManager.Instance != null)
            {
                var hierarchyManager = EcsHierarchyManager.Instance;
                var entityGO = hierarchyManager.ForceCreateEntityGameObject(entity, world);
                
                if (entityGO != null)
                {
                    EditorGUIUtility.PingObject(entityGO);
                }
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Entity:", GUILayout.Width(100));
        string newEntityName = EditorGUILayout.TextField(scenario.EntityName);
        if (newEntityName != scenario.EntityName)
        {
            scenario.SetEntityName(newEntityName);
        }
        EditorGUILayout.EndHorizontal();
        
        if (!string.IsNullOrEmpty(scenario.EntityName))
        {
            var entity = scenario.ParseEntity(scenario.EntityName);
            if (entity.IsValid && scenario.IsEntityValid(entity, scenario.WorldName))
            {
                var components = scenario.GetEntityComponents(entity, scenario.WorldName);
                if (components != null && components.Length > 0)
                {
                    EditorGUILayout.LabelField("Components on Entity:");
                    EditorGUI.indentLevel++;
                    foreach (var compInfo in components)
                    {
                        EditorGUILayout.LabelField($"- {compInfo.ComponentType.Name} {GetComponentDisplay(compInfo)}");
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("No components on entity");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Invalid entity or entity does not exist", MessageType.Warning);
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Component", GUILayout.Height(25)))
        {
            showComponentPopup = !showComponentPopup;
        }
        
        if (GUILayout.Button("Remove Last Component", GUILayout.Height(25)))
        {
            if (!string.IsNullOrEmpty(scenario.EntityName))
            {
                var entity = scenario.ParseEntity(scenario.EntityName);
                if (entity.IsValid && scenario.IsEntityValid(entity, scenario.WorldName))
                {
                    var components = scenario.GetEntityComponents(entity, scenario.WorldName);
                    if (components != null && components.Length > 0)
                    {
                        var lastComp = components[components.Length - 1];
                        RemoveComponentByType(entity, lastComp.ComponentType);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (showComponentPopup)
        {
            DrawComponentSelection();
        }
    }
    
    private void DrawComponentSelection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Component Selection", EditorStyles.boldLabel);
        
        selectedComponentIndex = EditorGUILayout.Popup("Component Type:", selectedComponentIndex, componentNames);
        string selectedComponent = componentNames[selectedComponentIndex];
        
        DrawComponentInput(selectedComponent);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            AddComponentToEntity(selectedComponent);
            showComponentPopup = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            showComponentPopup = false;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawComponentInput(string componentName)
    {
        switch (componentName)
        {
            case "TestComponent":
                componentInputs["TestComponent"] = EditorGUILayout.IntField("Value:", (int)componentInputs["TestComponent"]);
                break;
            case "Position":
                componentInputs["Position"] = EditorGUILayout.Vector3Field("Position:", (Vector3)componentInputs["Position"]);
                break;
            case "Velocity":
                componentInputs["Velocity"] = EditorGUILayout.Vector3Field("Velocity:", (Vector3)componentInputs["Velocity"]);
                break;
            case "Health":
                componentInputs["Health"] = EditorGUILayout.FloatField("Amount:", (float)componentInputs["Health"]);
                break;
            case "CounterComponent":
                componentInputs["CounterComponent"] = EditorGUILayout.IntField("Value:", (int)componentInputs["CounterComponent"]);
                break;
            case "UpdateCounter":
                componentInputs["UpdateCounter"] = EditorGUILayout.IntField("Value:", (int)componentInputs["UpdateCounter"]);
                break;
            case "FixedUpdateCounter":
                componentInputs["FixedUpdateCounter"] = EditorGUILayout.IntField("Value:", (int)componentInputs["FixedUpdateCounter"]);
                break;
            case "Spawner":
                componentInputs["Spawner"] = EditorGUILayout.IntField("SpawnCount:", (int)componentInputs["Spawner"]);
                break;
            case "Acceleration":
                componentInputs["Acceleration"] = EditorGUILayout.Vector3Field("Acceleration:", (Vector3)componentInputs["Acceleration"]);
                break;
        }
    }
    
    private void AddComponentToEntity(string componentName)
    {
        if (string.IsNullOrEmpty(scenario.EntityName))
        {
            EditorUtility.DisplayDialog("Error", "No entity selected. Create an entity first.", "OK");
            return;
        }
        
        var entity = scenario.ParseEntity(scenario.EntityName);
        if (!entity.IsValid || !scenario.IsEntityValid(entity, scenario.WorldName))
        {
            EditorUtility.DisplayDialog("Error", "Invalid entity or entity does not exist.", "OK");
            return;
        }
        
        try
        {
            switch (componentName)
            {
                case "TestComponent":
                    scenario.AddComponent(entity, new TestComponent { Value = (int)componentInputs["TestComponent"] }, scenario.WorldName);
                    break;
                case "Position":
                    var pos = (Vector3)componentInputs["Position"];
                    scenario.AddComponent(entity, new Position { X = pos.x, Y = pos.y, Z = pos.z }, scenario.WorldName);
                    break;
                case "Velocity":
                    var vel = (Vector3)componentInputs["Velocity"];
                    scenario.AddComponent(entity, new Velocity { X = vel.x, Y = vel.y, Z = vel.z }, scenario.WorldName);
                    break;
                case "Health":
                    scenario.AddComponent(entity, new Health { Amount = (float)componentInputs["Health"] }, scenario.WorldName);
                    break;
                case "Dead":
                    scenario.AddComponent(entity, new Dead(), scenario.WorldName);
                    break;
                case "Destroyed":
                    scenario.AddComponent(entity, new Destroyed(), scenario.WorldName);
                    break;
                case "CounterComponent":
                    scenario.AddComponent(entity, new CounterComponent { Value = (int)componentInputs["CounterComponent"] }, scenario.WorldName);
                    break;
                case "UpdateCounter":
                    scenario.AddComponent(entity, new UpdateCounter { Value = (int)componentInputs["UpdateCounter"] }, scenario.WorldName);
                    break;
                case "FixedUpdateCounter":
                    scenario.AddComponent(entity, new FixedUpdateCounter { Value = (int)componentInputs["FixedUpdateCounter"] }, scenario.WorldName);
                    break;
                case "Spawner":
                    scenario.AddComponent(entity, new Spawner { SpawnCount = (int)componentInputs["Spawner"] }, scenario.WorldName);
                    break;
                case "Acceleration":
                    var acc = (Vector3)componentInputs["Acceleration"];
                    scenario.AddComponent(entity, new Acceleration { X = acc.x, Y = acc.y, Z = acc.z }, scenario.WorldName);
                    break;
            }
            
            Debug.Log($"Added {componentName} to entity {scenario.EntityName}");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to add component: {e.Message}", "OK");
        }
    }
    
    private void RemoveComponentByType(Entity entity, Type componentType)
    {
        try
        {
            bool removed = false;
            switch (componentType.Name)
            {
                case "TestComponent":
                    removed = scenario.RemoveComponent<TestComponent>(entity, scenario.WorldName);
                    break;
                case "Position":
                    removed = scenario.RemoveComponent<Position>(entity, scenario.WorldName);
                    break;
                case "Velocity":
                    removed = scenario.RemoveComponent<Velocity>(entity, scenario.WorldName);
                    break;
                case "Health":
                    removed = scenario.RemoveComponent<Health>(entity, scenario.WorldName);
                    break;
                case "Dead":
                    removed = scenario.RemoveComponent<Dead>(entity, scenario.WorldName);
                    break;
                case "Destroyed":
                    removed = scenario.RemoveComponent<Destroyed>(entity, scenario.WorldName);
                    break;
                case "CounterComponent":
                    removed = scenario.RemoveComponent<CounterComponent>(entity, scenario.WorldName);
                    break;
                case "UpdateCounter":
                    removed = scenario.RemoveComponent<UpdateCounter>(entity, scenario.WorldName);
                    break;
                case "FixedUpdateCounter":
                    removed = scenario.RemoveComponent<FixedUpdateCounter>(entity, scenario.WorldName);
                    break;
                case "Spawner":
                    removed = scenario.RemoveComponent<Spawner>(entity, scenario.WorldName);
                    break;
                case "Acceleration":
                    removed = scenario.RemoveComponent<Acceleration>(entity, scenario.WorldName);
                    break;
            }
            
            if (removed)
            {
                Debug.Log($"Removed {componentType.Name} from entity {scenario.EntityName}");
            }
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to remove component: {e.Message}", "OK");
        }
    }
    
    private string GetComponentDisplay(ComponentInfo compInfo)
    {
        var entity = scenario.ParseEntity(scenario.EntityName);
        if (!entity.IsValid) return "";
        
        try
        {
            var world = scenario.GetWorld();
            switch (compInfo.ComponentType.Name)
            {
                case "TestComponent":
                    var tc = world.GetComponent<TestComponent>(entity);
                    return $"(Value: {tc.Value})";
                case "Position":
                    var pos = world.GetComponent<Position>(entity);
                    return $"(X: {pos.X:F2}, Y: {pos.Y:F2}, Z: {pos.Z:F2})";
                case "Velocity":
                    var vel = world.GetComponent<Velocity>(entity);
                    return $"(X: {vel.X:F2}, Y: {vel.Y:F2}, Z: {vel.Z:F2})";
                case "Health":
                    var health = world.GetComponent<Health>(entity);
                    return $"(Amount: {health.Amount:F2})";
                case "CounterComponent":
                    var cc = world.GetComponent<CounterComponent>(entity);
                    return $"(Value: {cc.Value})";
                case "UpdateCounter":
                    var uc = world.GetComponent<UpdateCounter>(entity);
                    return $"(Value: {uc.Value})";
                case "FixedUpdateCounter":
                    var fuc = world.GetComponent<FixedUpdateCounter>(entity);
                    return $"(Value: {fuc.Value})";
                case "Spawner":
                    var spawner = world.GetComponent<Spawner>(entity);
                    return $"(SpawnCount: {spawner.SpawnCount})";
                case "Acceleration":
                    var acc = world.GetComponent<Acceleration>(entity);
                    return $"(X: {acc.X:F2}, Y: {acc.Y:F2}, Z: {acc.Z:F2})";
                default:
                    return "";
            }
        }
        catch
        {
            return "";
        }
    }
    
    private void DrawSystemManagement()
    {
        EditorGUILayout.LabelField("System Management", EditorStyles.boldLabel);
        
        selectedSystemIndex = EditorGUILayout.Popup("System Type:", selectedSystemIndex, systemNames);
        string selectedSystem = systemNames[selectedSystemIndex];
        
        if (selectedSystem == "SetValueSystem")
        {
            setValueSystemInput = EditorGUILayout.IntField("Value to Set:", setValueSystemInput);
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add to Update", GUILayout.Height(25)))
        {
            AddSystemToQueue(selectedSystem, "Update");
        }
        if (GUILayout.Button("Add to FixedUpdate", GUILayout.Height(25)))
        {
            AddSystemToQueue(selectedSystem, "FixedUpdate");
        }
        EditorGUILayout.EndHorizontal();
        
        var world = scenario.GetWorld();
        var updateQueue = world.GetUpdateQueue();
        var fixedUpdateQueue = world.GetFixedUpdateQueue();
        
        if (updateQueue.Count > 0 || fixedUpdateQueue.Count > 0)
        {
            EditorGUILayout.LabelField("Active Systems:");
            EditorGUI.indentLevel++;
            
            var systemsToRemove = new List<(SystemHandler system, string queue)>();
            
            foreach (var system in updateQueue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"- {system.GetType().Name} (Update)");
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    systemsToRemove.Add((system, "Update"));
                }
                EditorGUILayout.EndHorizontal();
            }
            
            foreach (var system in fixedUpdateQueue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"- {system.GetType().Name} (FixedUpdate)");
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    systemsToRemove.Add((system, "FixedUpdate"));
                }
                EditorGUILayout.EndHorizontal();
            }
            
            foreach (var (system, queue) in systemsToRemove)
            {
                scenario.RemoveSystem(system, scenario.WorldName, queue);
            }
            
            if (systemsToRemove.Count > 0)
            {
                EditorApplication.delayCall += () =>
                {
                    if (EcsHierarchyManager.Instance != null)
                    {
                        EcsHierarchyManager.Instance.UpdateHierarchy();
                    }
                };
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void AddSystemToQueue(string systemName, string queue)
    {
        try
        {
            SystemHandler system = null;
            
            switch (systemName)
            {
                case "IncrementSystem":
                    system = new IncrementSystem();
                    break;
                case "MovementSystem":
                    system = new MovementSystem();
                    break;
                case "HealthSystem":
                    system = new HealthSystem();
                    break;
                case "ModifiableHealthSystem":
                    system = new ModifiableHealthSystem();
                    break;
                case "PhysicsSystem":
                    system = new PhysicsSystem();
                    break;
                case "SetValueSystem":
                    system = new SetValueSystem(setValueSystemInput);
                    break;
                case "UpdateCounterSystem":
                    system = new UpdateCounterSystem();
                    break;
                case "FixedUpdateCounterSystem":
                    system = new FixedUpdateCounterSystem();
                    break;
                case "CleanupSystem":
                    system = new CleanupSystem();
                    break;
                case "SpawnSystem":
                    system = new SpawnSystem();
                    break;
            }
            
            if (system != null)
            {
                scenario.AddSystem(system, scenario.WorldName, queue);
                Debug.Log($"Added {systemName} to {queue} queue");
            }
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to add system: {e.Message}", "OK");
        }
    }
}
#endif

