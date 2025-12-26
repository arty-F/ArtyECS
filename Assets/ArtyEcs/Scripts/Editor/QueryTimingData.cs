#if UNITY_EDITOR
using System;

namespace ArtyECS.Core
{
    public struct QueryTimingData
    {
        public QueryType QueryType { get; set; }
        public WorldInstance World { get; set; }
        public double LastExecutionTime { get; set; }
        public double TotalExecutionTime { get; set; }
        public long ExecutionCount { get; set; }
        public double MaxExecutionTime { get; set; }
        public int InsertionOrder { get; set; }
        public double AverageTime => ExecutionCount > 0 ? TotalExecutionTime / ExecutionCount : 0.0;

        public QueryTimingData(QueryType queryType, WorldInstance world)
        {
            QueryType = queryType;
            World = world;
            LastExecutionTime = 0.0;
            TotalExecutionTime = 0.0;
            ExecutionCount = 0;
            MaxExecutionTime = 0.0;
            InsertionOrder = 0;
        }
    }
}
#endif

