using System;

[Serializable]
public struct TestResult
{
    public bool Passed;
    public string Message;
    public DateTime LastRunTime;
    
    public TestResult(bool passed, string message)
    {
        Passed = passed;
        Message = message;
        LastRunTime = DateTime.Now;
    }
}

