using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRTX.Build
{

    public class BuildBridgePreferences
    {

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("Unity Build Bridge")]
        public static void PreferencesGUI()
        {
            EditorGUILayout.HelpBox("The Unity Build Bridge is an Unity editor extension to simplify the use of iOS and Android build pipelines on from within the Unity editor."
                + Environment.NewLine + "To build iOS applications the iOS Build Environment is required."
                + Environment.NewLine + "To build Android applications the Gradle and Android SDK is required."
                , MessageType.None, true);

            EditorGUILayout.Separator();
            GUILayout.Label("iOS", EditorStyles.boldLabel);
            BuildBridgeIOS.Preferences.PreferencesGUI();

            EditorGUILayout.Separator();
            GUILayout.Label("Android", EditorStyles.boldLabel);
            BuildBridgeAndroid.Preferences.PreferencesGUI();
        }


        public class Utilitites
        {
            private const float MINIBUTTONWIDTH = 50.0f;

            public static bool DrawMiniButton(string label)
            { return GUILayout.Button(label, EditorStyles.miniButton, GUILayout.MaxWidth(MINIBUTTONWIDTH)); }

        }
    }

}
