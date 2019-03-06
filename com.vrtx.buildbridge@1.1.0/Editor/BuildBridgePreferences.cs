using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRTX.Build
{
    public class BuildBridgePreferences
    {

        // constant fields (should not change between versions)
        private const string PKey_EnvironmentPath = "VRTX.BuildBridge.iOS.EnvironmentPath";
        private const string BuildEnv = "iOS Build Environment";


        // Have we loaded the prefs yet
        private static bool _prefsLoaded = false;
        private static string _environmentPath = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), BuildEnv);

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("Build Bridge")]
        public static void PreferencesGUI()
        {
            // Load the preferences
            if (!_prefsLoaded)
            {
                _environmentPath = EditorPrefs.GetString(PKey_EnvironmentPath, _environmentPath);
                _prefsLoaded = true;
            }

            GUILayout.Label("Path");
            using (new EditorGUILayout.HorizontalScope())
            {
                // Preferences GUI
                _environmentPath = EditorGUILayout.TextField(_environmentPath);

                //open folder dialog for build environment path selection
                if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.MaxWidth(25.0f)))
                {
                    string selectedPath = _environmentPath;
                    if (!Directory.Exists(selectedPath))
                        selectedPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%");
                    string newSelectedPath = EditorUtility.OpenFolderPanel("Select path to build environment", selectedPath, string.Empty);
                    if (!selectedPath.Equals(newSelectedPath) && Directory.Exists(newSelectedPath))
                        _environmentPath = newSelectedPath;
                }
            }

            // Save the preferences
            if (UnityEngine.GUI.changed)
                EditorPrefs.SetString(PKey_EnvironmentPath, _environmentPath);
        }

        public static string EnvironmentPath
        {
            get
            {
                //if (!_prefsLoaded)
                _environmentPath = EditorPrefs.GetString(PKey_EnvironmentPath, _environmentPath);
                return _environmentPath;
            }
        }

    }

}
