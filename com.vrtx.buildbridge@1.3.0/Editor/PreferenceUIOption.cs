using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRTX.Build
{
    public class PreferenceUIOption
    {
        public abstract class Base
        {
            public abstract void DrawUI();
        }

        public abstract class Base<T> : Base where T : IConvertible
        {
            protected static readonly Type TGeneric = typeof(T);

            protected string _prefKey = string.Empty;
            protected T _value = default(T);
            protected T _defaultValue = default(T);
            protected GUIContent _guiContent = null;


            public T Value
            {
                get
                {
                    if (_value is bool)
                        _value = (T)Convert.ChangeType(EditorPrefs.GetBool(_prefKey, Convert.ToBoolean(_value)), TGeneric);
                    else if (_value is int)
                        _value = (T)Convert.ChangeType(EditorPrefs.GetInt(_prefKey, Convert.ToInt32(_value)), TGeneric);
                    else if (_value is float)
                        _value = (T)Convert.ChangeType(EditorPrefs.GetFloat(_prefKey, Convert.ToSingle(_value)), TGeneric);
                    else if (_value is string)
                        _value = (T)Convert.ChangeType(EditorPrefs.GetString(_prefKey, Convert.ToString(_value)), TGeneric);
                    else
                        Debug.LogWarning(TGeneric.Name + " type is not supported in EditorPrefs and will not be retrieved from them.");
                    return _value;
                }
                set
                {
                    if (_value is bool)
                        EditorPrefs.SetBool(_prefKey, Convert.ToBoolean(_value));
                    else if (_value is int)
                        EditorPrefs.SetInt(_prefKey, Convert.ToInt32(_value));
                    else if (_value is float)
                        EditorPrefs.SetFloat(_prefKey, Convert.ToSingle(_value));
                    else if (_value is string)
                        EditorPrefs.SetString(_prefKey, Convert.ToString(_value));
                    else
                        Debug.LogWarning(TGeneric.Name + " type is not supported in EditorPrefs and will not be saved between editor sessions.");
                }
            }

            public Base(string preferenceKey, T defaultValue, string label, string tooltip = "")
            {
                _prefKey = preferenceKey;
                _value = defaultValue;
                _defaultValue = defaultValue;
                _guiContent = new GUIContent(label, tooltip);
            }

        }

        public class Separator : Base
        {
            public override void DrawUI()
            { EditorGUILayout.Separator(); }
        }
        public class Title : Base
        {
            protected GUIContent _guiContent = null;
            public Title(string label, string tooltip = "")
            {
                _guiContent = new GUIContent(label, tooltip);
            }

            public override void DrawUI()
            {
                // clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(_guiContent, EditorStyles.boldLabel);
                }
            }
        }


        public class Bool : Base<bool>
        {
            public Bool(string preferenceKey, bool defaultValue, string label, string tooltip = "") : base(preferenceKey, defaultValue, label, tooltip)
            { }

            public override void DrawUI()
            {
                // clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    _value = EditorGUILayout.Toggle(_guiContent, Value);
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("Reset")) _value = _defaultValue;
                }
                // Save the preferences
                if (UnityEngine.GUI.changed)
                    this.Value = _value;
            }
        }
        // To-Do:
        // continue implementing the other preference types
        public class Int : Base<int>
        {
            public Int(string preferenceKey, int defaultValue, string label, string tooltip = "") : base(preferenceKey, defaultValue, label, tooltip)
            { }
            public override void DrawUI()
            {
                //clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    _value = EditorGUILayout.IntField(_guiContent, Value);
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("Reset")) _value = _defaultValue;
                }
                // Save the preferences
                if (UnityEngine.GUI.changed)
                    this.Value = _value;
            }
        }
        public class Float : Base<float>
        {
            public Float(string preferenceKey, float defaultValue, string label, string tooltip = "") : base(preferenceKey, defaultValue, label, tooltip)
            { }
            public override void DrawUI()
            {
                //clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    _value = EditorGUILayout.FloatField(_guiContent, Value);
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("Reset")) _value = _defaultValue;
                }
                // Save the preferences
                if (UnityEngine.GUI.changed)
                    this.Value = _value;
            }
        }
        public class String : Base<string>
        {
            public String(string preferenceKey, string defaultValue, string label, string tooltip = "") : base(preferenceKey, defaultValue, label, tooltip)
            { }
            public override void DrawUI()
            {
                // clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    _value = EditorGUILayout.TextField(_guiContent, Value);
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("Reset")) _value = _defaultValue;
                }
                // Save the preferences
                if (UnityEngine.GUI.changed)
                    this.Value = _value;
            }
        }
        // special implementation of the String option class which allows picking a path via system dialog
        public class Folder : String
        {
            protected string _dialogTitle = string.Empty;
            protected string _dialogFallback = string.Empty;

            public Folder(string preferenceKey, string defaultValue, string label, string tooltip = "", string dialogTitle = "", string dialogFallback = "") : base(preferenceKey, defaultValue, label, tooltip)
            {
                _dialogTitle = dialogTitle;
                _dialogFallback = string.IsNullOrEmpty(dialogFallback) ? Application.dataPath : dialogFallback;
            }

            public override void DrawUI()
            {
                // clean compiler cache
                using (new EditorGUILayout.HorizontalScope())
                {
                    _value = EditorGUILayout.TextField(_guiContent, Value);
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("...")) SelectPath();
                    if (BuildBridgePreferences.Utilitites.DrawMiniButton("Reset")) _value = _defaultValue;
                }
                // Save the preferences
                if (UnityEngine.GUI.changed)
                    this.Value = _value;
            }

            private void SelectPath()
            {
                string selectedPath = _value;
                if (!Directory.Exists(selectedPath))
                    selectedPath = Environment.ExpandEnvironmentVariables(_dialogFallback);
                string newSelectedPath = EditorUtility.OpenFolderPanel(_dialogTitle, selectedPath, string.Empty);
                if (!selectedPath.Equals(newSelectedPath) && Directory.Exists(newSelectedPath))
                    _value = newSelectedPath;
            }
        }
    }

}
