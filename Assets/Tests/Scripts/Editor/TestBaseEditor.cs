#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(TestBase), true)]
public class TestBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TestBase testBase = (TestBase)target;
        
        // Header with test class name
        EditorGUILayout.LabelField($"ArtyECS Tests - {testBase.GetType().Name}", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // "Run All Tests" button
        if (GUILayout.Button("Run All Tests", GUILayout.Height(30)))
        {
            testBase.RunAllTests();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Individual Tests:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Get all test methods through reflection
        MethodInfo[] testMethods = GetTestMethods(testBase);
        
        // Sort methods by name
        testMethods = testMethods.OrderBy(m => m.Name).ToArray();
        
        // Display each test
        foreach (MethodInfo method in testMethods)
        {
            DrawTestButton(testBase, method);
        }
    }
    
    private MethodInfo[] GetTestMethods(TestBase testBase)
    {
        return testBase.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("Test_"))
            .ToArray();
    }
    
    private void DrawTestButton(TestBase testBase, MethodInfo method)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Get test name from ContextMenu attribute or use method name
        string testName = GetTestDisplayName(method);
        
        // Get test result
        TestResult? result = testBase.GetTestResult(method.Name);
        
        // Status indicator (colored square)
        Color statusColor = result.HasValue 
            ? (result.Value.Passed ? Color.green : Color.red)
            : Color.gray;
        
        Rect statusRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
        EditorGUI.DrawRect(statusRect, statusColor);
        
        // Test name
        EditorGUILayout.LabelField(testName, GUILayout.ExpandWidth(true));
        
        // "Run Test" button
        if (GUILayout.Button("Run Test", GUILayout.Width(80)))
        {
            method.Invoke(testBase, null);
            EditorUtility.SetDirty(testBase);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Show result message if available
        if (result.HasValue && !string.IsNullOrEmpty(result.Value.Message))
        {
            EditorGUILayout.HelpBox(result.Value.Message, 
                result.Value.Passed ? MessageType.Info : MessageType.Error);
        }
        
        EditorGUILayout.Space(5);
    }
    
    private string GetTestDisplayName(MethodInfo method)
    {
        // Try to get name from ContextMenu attribute
        var contextMenuAttr = method.GetCustomAttribute<ContextMenu>();
        if (contextMenuAttr != null)
        {
            // Remove "Run Test: " prefix if present
            string name = contextMenuAttr.menuItem;
            if (name.StartsWith("Run Test: "))
            {
                return name.Substring("Run Test: ".Length);
            }
            return name;
        }
        
        // Otherwise use method name, removing Test_ prefix and replacing _ with spaces
        string methodName = method.Name;
        if (methodName.StartsWith("Test_"))
        {
            methodName = methodName.Substring("Test_".Length);
        }
        return methodName.Replace("_", " ");
    }
}
#endif

