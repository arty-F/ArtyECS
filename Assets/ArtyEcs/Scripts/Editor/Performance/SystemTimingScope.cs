#if UNITY_EDITOR
using System;
using System.Diagnostics;

namespace ArtyECS.Core
{
    public struct SystemTimingScope : IDisposable
    {
        private readonly SystemHandler _system;
        private readonly WorldInstance _world;
        private readonly Stopwatch _stopwatch;
        private readonly bool _isEnabled;

        internal SystemTimingScope(SystemHandler system, WorldInstance world)
        {
            _system = system;
            _world = world;
            _isEnabled = PerformanceMonitoring.IsEnabled;
            _stopwatch = _isEnabled ? Stopwatch.StartNew() : null;
        }

        public void Dispose()
        {
            if (_isEnabled && _stopwatch != null)
            {
                _stopwatch.Stop();
                PerformanceMonitoring.RecordSystemTiming(_system, _world, _stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
#endif

