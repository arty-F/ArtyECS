#if UNITY_EDITOR
using UnityEngine;

namespace ArtyECS.Core
{
    public static class PerformanceMonitoring
    {
        private const string PREFS_KEY_MONITORING_ENABLED = "ArtyECS.PerformanceMonitoring.Enabled";
        private const string PREFS_KEY_SHOW_WARNINGS = "ArtyECS.PerformanceMonitoring.ShowWarnings";
        private static bool _isEnabled = false;
        private static bool _showWarnings = false;

        static PerformanceMonitoring()
        {
            _isEnabled = UnityEditor.EditorPrefs.GetBool(PREFS_KEY_MONITORING_ENABLED, false);
            _showWarnings = UnityEditor.EditorPrefs.GetBool(PREFS_KEY_SHOW_WARNINGS, true);
        }

        public static bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    UnityEditor.EditorPrefs.SetBool(PREFS_KEY_MONITORING_ENABLED, value);
                }
            }
        }

        public static bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                if (_showWarnings != value)
                {
                    _showWarnings = value;
                    UnityEditor.EditorPrefs.SetBool(PREFS_KEY_SHOW_WARNINGS, value);
                }
            }
        }
    }
}
#endif

