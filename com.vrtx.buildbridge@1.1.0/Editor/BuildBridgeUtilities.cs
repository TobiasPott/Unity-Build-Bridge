using System;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace VRTX.Build
{
    public class BuildBridgeUtilities
    {
        internal static System.Diagnostics.Process CreateProcess(string command, string arguments, Action callback = null)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(command, arguments);
            p.EnableRaisingEvents = true;
            p.Exited += (object sender, System.EventArgs e) =>
            { if (callback != null) callback.Invoke(); };
            return p;
        }
        private static bool BuildSourceProject(string path, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
#if UNITY_2018_1_OR_NEWER
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();

            // set editor build scene list
            buildOptions.scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < buildOptions.scenes.Length; i++)
                buildOptions.scenes[i] = EditorBuildSettings.scenes[i].path;

            buildOptions.target = target;
            buildOptions.locationPathName = path;
            buildOptions.options = options;
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            UnityEngine.Debug.Log("Unity build result: " + report.steps.Length);
#else
            UnityEngine.Debug.Log("Unity build result: " + BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, target, options));
#endif
            // ! ! ! !
            // need to check results to provide accurate return value
            return true;
        }

        internal static bool BuildSource(string path, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
            return BuildSourceProject(path, target, options);
        }
        internal static bool BuildSourceDevMode(string path, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
            return BuildSourceProject(path, target, options | BuildOptions.Development);
        }
        internal static bool BuildSourceAppendMode(string path, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
            return BuildSourceProject(path, target, options | BuildOptions.AcceptExternalModificationsToPlayer);
        }
    }

}