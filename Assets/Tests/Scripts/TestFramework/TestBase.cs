using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Base class for all test classes in ArtyECS test framework.
/// Provides assertion methods and test result management.
/// </summary>
public abstract class TestBase : MonoBehaviour
{
    // Dictionary to store test results
    protected Dictionary<string, TestResult> testResults = new Dictionary<string, TestResult>();
    
    // ========== Assert Methods ==========
    
    protected void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new AssertionException($"Assertion failed: {message}");
        }
    }
    
    protected void AssertEquals<T>(T expected, T actual, string message = "")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new AssertionException($"Expected {expected}, but got {actual}. {message}");
        }
    }
    
    protected void AssertNotNull<T>(T value, string message = "") where T : class
    {
        if (value == null)
        {
            throw new AssertionException($"Expected non-null value. {message}");
        }
    }
    
    protected void AssertNull<T>(T value, string message = "") where T : class
    {
        if (value != null)
        {
            throw new AssertionException($"Expected null value, but got {value}. {message}");
        }
    }
    
    // ========== Test Execution Helpers ==========
    
    /// <summary>
    /// Executes a test method and handles result storage.
    /// Call this at the start of each test method.
    /// </summary>
    protected void ExecuteTest(string testName, Action testAction)
    {
        try
        {
            Debug.Log($"[TEST] Starting: {testName}");
            testAction();
            Debug.Log($"[TEST] PASSED: {testName}");
            testResults[testName] = new TestResult(true, "Test passed");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TEST] FAILED: {testName} - {ex.Message}");
            Debug.LogException(ex);
            testResults[testName] = new TestResult(false, ex.Message);
        }
    }
    
    // ========== Inspector Support Methods ==========
    
    public TestResult? GetTestResult(string testName)
    {
        if (testResults.TryGetValue(testName, out TestResult result))
        {
            return result;
        }
        return null;
    }
    
    public void RunAllTests()
    {
        Debug.Log($"[TEST] ========== Running All Tests in {GetType().Name} ==========");
        
        // Get all test methods through reflection
        MethodInfo[] testMethods = GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("Test_"))
            .OrderBy(m => m.Name)
            .ToArray();
        
        int passed = 0;
        int failed = 0;
        
        foreach (MethodInfo method in testMethods)
        {
            try
            {
                method.Invoke(this, null);
                
                if (testResults.TryGetValue(method.Name, out TestResult result))
                {
                    if (result.Passed)
                        passed++;
                    else
                        failed++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TEST] Exception running {method.Name}: {ex.Message}");
                failed++;
            }
        }
        
        Debug.Log($"[TEST] ========== Tests Complete ==========");
        Debug.Log($"[TEST] Passed: {passed}, Failed: {failed}, Total: {testMethods.Length}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

