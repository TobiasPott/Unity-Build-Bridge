using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace VRTX.Build
{
    public class BuildBridgeUtilities
    {
        /// <summary>
        /// creates a new process instance for the given command/filename and arguments list combination
        /// </summary>
        /// <param name="command">file or command which should be executed when the process is started</param>
        /// <param name="arguments">arguments passed to the file or command (can be left empty)</param>
        /// <param name="callback">callback which is executed when the process exited (this cannot contain every Unity3D references as the processes are started as separate threads and cannot access them)</param>
        /// <returns>the Process instance created with the given parameters</returns>
        internal static System.Diagnostics.Process CreateProcess(string command, string arguments, Action callback = null)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(command, arguments);
            p.EnableRaisingEvents = true;
            p.Exited += (object sender, System.EventArgs e) =>
            { if (callback != null) callback.Invoke(); };
            return p;
        }


        private static BuildReport BuildSourceProject(string path, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();

            // set editor build scene list
            buildOptions.scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < buildOptions.scenes.Length; i++)
                buildOptions.scenes[i] = EditorBuildSettings.scenes[i].path;

            buildOptions.target = target;
            buildOptions.locationPathName = path;
            buildOptions.options = options;
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            return report;
            // ! ! ! !
            // need to check results to provide accurate return value
        }

        internal static bool BuildSourceWithReport(string path, BuildTarget target, BuildOptions options, out BuildReport report)
        {
            report = BuildSourceProject(path, target, options);
            return report.summary.result == BuildResult.Succeeded;
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