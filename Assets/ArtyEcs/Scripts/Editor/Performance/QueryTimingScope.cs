#if UNITY_EDITOR
using System;
using System.Diagnostics;

namespace ArtyECS.Core
{
    public struct QueryTimingScope : IDisposable
    {
        private readonly QueryType _queryType;
        private readonly WorldInstance _world;
        private readonly string _componentTypes;
        private readonly Stopwatch _stopwatch;
        private readonly bool _isEnabled;

        internal QueryTimingScope(QueryType queryType, WorldInstance world, string componentTypes = null)
        {
            _queryType = queryType;
            _world = world;
            _componentTypes = componentTypes;
            _isEnabled = PerformanceMonitoring.IsEnabled;
            _stopwatch = _isEnabled ? Stopwatch.StartNew() : null;
        }

        public void Dispose()
        {
            if (_isEnabled && _stopwatch != null)
            {
                _stopwatch.Stop();
                PerformanceMonitoring.RecordQueryTiming(_queryType, _world, _stopwatch.Elapsed.TotalMilliseconds, _componentTypes);
            }
        }
    }
}
#endif

