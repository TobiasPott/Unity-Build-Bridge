using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace VRTX.Build
{
    public class BuildBridgeAndroid : BuildBridgeBase<BuildBridgeAndroid>
    {
        private const string DeployCommandPattern = "\"{0}\""; // insert PathADB value
        private const string DeployArgumentsPattern = "install -r " + "\"{0}\""; // insert path-to-APK value

        private const string LaunchCommandPattern = "\"{0}\""; // insert PathADB value
        private const string LaunchArgumentsPattern = "shell monkey -p {0} -c android.intent.category.LAUNCHER 1"; // insert PlayerSettings.applicationIdentifier value

        private const string CMDGradle = "gradle";
        private const string CMDGradleParamClean = "clean";
        private const string CMDGradleParamAssembleDebug = "assembleDebug";
        private const string CMDGradleParamAssembleRelease = "assembleRelease";

        private const string GradleBuildFileName = "build.gradle";
        private const string GradlePropertiesFileName = "gradle.properties";



        private static string _pathProject = string.Empty;
        private static string _outputPathGradle = string.Empty;
        private static string _outputNameAPK = string.Empty;

        public static string PathProject
        {
            get
            {
                if (_diProject == null)
                    _diProject = new DirectoryInfo(UnityEngine.Application.dataPath).Parent;
                if (!_diProject.Exists)
                    _diProject.Create();
                if (_pathProject.Equals(string.Empty))
                    _pathProject = _diProject.FullName;
                return _pathProject;
            }
        }
        public static string OutputPathGradle
        {
            get
            {
                if (_outputPathGradle.Equals(string.Empty))
                    _outputPathGradle = Path.Combine(PathProject, "Gradle");
                if (!Directory.Exists(_outputPathGradle))
                    Directory.CreateDirectory(_outputPathGradle);
                return _outputPathGradle;
            }
        }
        public static string OutputNameAPK
        {
            get
            {
                if (String.IsNullOrEmpty(_outputNameAPK))
                    _outputNameAPK = PlayerSettings.productName + ".apk";
                return _outputNameAPK;
            }
        }

        public static string PathSDK
        { get { return EditorPrefs.GetString("AndroidSdkRoot"); } }
        public static string PathADB
        { get { return Path.Combine(PathSDK, "platform-tools/adb.exe"); } }


#if UNITY_EDITOR
        [MenuItem(BuildBridgeMenu.MenuBase + "Generate (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 1)]
        public new static void BuildBridgeGenerate()
        { Instance.Generate(BuildOptions.None, null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Build (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 2)]
        public new static void BuildBridgeBuild()
        {
            BuildBridgeAndroid.Prepare();
            Instance.BuildAsync("", null);
        }

        [MenuItem(BuildBridgeMenu.MenuBase + "Deploy (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 3)]
        public new static void BuildBridgeDeploy()
        {
            BuildBridgeAndroid.Prepare();
            Instance.DeployAsync(null);
        }

        [MenuItem(BuildBridgeMenu.MenuBase + "Generate, Build and Deploy (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 4)]
        public new static void GenerateBuildAndDeploy()
        { Instance.BuildStepsAsync(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build | IBuildBridgeSteps.Deploy, BuildOptions.None, ""); }

        public new static void GenerateAndBuild()
        { Instance.BuildSteps(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build, BuildOptions.None, "", null, null, null); }
#endif

        private static void DeployProcess_Exited(object sender, System.EventArgs e)
        {
            UnityEngine.Debug.Log("ADB Deploy finished!");
        }
        private static void LaunchProcess_Exited(object sender, System.EventArgs e)
        {
            UnityEngine.Debug.Log("ADB Launch finished!");
        }


        public override bool Generate(BuildOptions options, Action callback = null)
        {
            BuildBridgeAndroid.Prepare();
            AndroidBuildSettings prevSettings = AndroidBuildSettings.StoreSettings();
            AndroidBuildSettings.ApplySettingsForGradleProject();
            BuildReport report = null;
            bool result = BuildBridgeUtilities.BuildSourceWithReport(BuildBridgeAndroid.OutputPathGradle, BuildTarget.Android, options, out report);
            AndroidBuildSettings.ApplySettings(prevSettings);
            if (result)
            {
                UnityEngine.Debug.Log("Build succeeded: " + report.summary.outputPath);

                if (callback != null) callback.Invoke();
            }
            return result;
        }
        public override bool Build(string args, Action callback = null)
        {
            BuildBridgeAndroid.Prepare();
            //return await BuildAsync(args, callback));
            //BuildBridgeAndroid.Prepare();
            //DirectoryInfo diGradle = new DirectoryInfo(OutputPathGradle);
            //FileInfo[] files = diGradle.GetFiles(GradleBuildFileName, SearchOption.AllDirectories);
            //if (files.Length > 0)
            //{
            //    string path = files[0].Directory.FullName;

            //    Process p = new Process();
            //    p.StartInfo = new ProcessStartInfo("cmd", "/C " + string.Join(" ", CMDGradle, CMDGradleParamClean, CMDGradleParamAssembleDebug));
            //    p.StartInfo.UseShellExecute = false;
            //    p.StartInfo.WorkingDirectory = path;
            //    // add machine "PATH" to processes "Path" (cannot override "PATH" directly but using the user's "Path" works)
            //    p.StartInfo.Environment.Add("Path", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine));

            //    if (BuildBridgeAndroid.IsBatchMode)
            //    {
            //        // redirect output to print it back to unity console
            //        p.StartInfo.RedirectStandardOutput = true;
            //    }

            //    if (p.Start())
            //    {
            //        UnityEngine.Debug.Log("Gradle Build started..");
            //        if (BuildBridgeAndroid.IsBatchMode)
            //            UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd());
            //        // wait for exit (will block unity main thread if "Build" method is called directly)
            //        p.WaitForExit();

            //        UnityEngine.Debug.Log("Gradle Build finished!");
            //        if (callback != null) callback.Invoke();
            //        return true;
            //    }
            //}
            //UnityEngine.Debug.Log("Gradle Build failed to start..");
            return false;
        }
        private async Task<bool> BuildAsync(string args, Action callback = null)
        {
            DirectoryInfo diGradle = new DirectoryInfo(OutputPathGradle);
            FileInfo[] files = diGradle.GetFiles(GradleBuildFileName, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                string path = files[0].Directory.FullName;

                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("cmd", "/C " + string.Join(" ", CMDGradle, CMDGradleParamClean, CMDGradleParamAssembleDebug));
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = path;
                // add machine "PATH" to processes "Path" (cannot override "PATH" directly but using the user's "Path" works)
                p.StartInfo.Environment.Add("Path", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine));

                // redirect output to print it back to unity console
                p.StartInfo.RedirectStandardOutput = true;

                if (p.Start())
                {
                    UnityEngine.Debug.Log("Gradle Build started..");

                    // wait for exit (will block unity main thread if "Build" method is called directly)
                    await Task.Run(() => p.WaitForExit());
                    //if (BuildBridgeAndroid.IsBatchMode)
                    UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd());

                    UnityEngine.Debug.Log("Gradle Build finished!");
                    if (callback != null) callback.Invoke();
                    return true;
                }
            }
            UnityEngine.Debug.LogWarning("Gradle Build failed to start..");
            return false;
        }

        public override bool Deploy(Action callback = null)
        {
            BuildBridgeAndroid.Prepare();
            DirectoryInfo diOutputPathGradle = new DirectoryInfo(OutputPathGradle);
            FileInfo fiGradleBuildFile = diOutputPathGradle.GetFiles(GradleBuildFileName, SearchOption.AllDirectories).FirstOrDefault();

            if (fiGradleBuildFile != null)
            {
                string gradleOutputPath = Path.Combine(fiGradleBuildFile.Directory.FullName, "build", "outputs", "apk"); // last is configuration and 
                DirectoryInfo diGradleOutputPath = new DirectoryInfo(gradleOutputPath);

                if (diGradleOutputPath.Exists)
                {
                    FileInfo[] files = diGradleOutputPath.GetFiles("*.apk", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        gradleOutputPath = files[0].FullName;

                        System.Diagnostics.Process pLaunch = BuildBridgeUtilities.CreateProcess(string.Format(LaunchCommandPattern, PathADB), string.Format(LaunchArgumentsPattern, BuildBridgeAndroid.AppIdentifier));
                        System.Diagnostics.Process pDeploy = BuildBridgeUtilities.CreateProcess(string.Format(DeployCommandPattern, PathADB), string.Format(DeployArgumentsPattern, files[0].FullName), () => { pLaunch.Start(); });

                        pDeploy.StartInfo.UseShellExecute = false;
                        pDeploy.StartInfo.RedirectStandardOutput = true;

                        if (pDeploy.Start())
                        {
                            if (callback != null)
                            {
                                UnityEngine.Debug.Log(pDeploy.StandardOutput.ReadToEnd());
                                callback.Invoke();
                            }
                            UnityEngine.Debug.Log("APK deployed to connected device..");
                            return true;
                        }
                    }
                }
            }
            return false;

            /*
            string apkPath = PathProject;

            FileInfo[] files = _diProject.GetFiles("*.apk", SearchOption.TopDirectoryOnly);
            Array.Sort(files, (x, y) => x.LastAccessTime.CompareTo(y.LastAccessTime));
            if (files.Length > 0)
            {
                apkPath = files[0].FullName;
                System.Diagnostics.Process pLaunch = BuildBridgeUtilities.CreateProcess(string.Format(LaunchCommandPattern, PathADB), string.Format(LaunchArgumentsPattern, PlayerSettings.applicationIdentifier));
                System.Diagnostics.Process pDeploy = BuildBridgeUtilities.CreateProcess(string.Format(DeployCommandPattern, PathADB), string.Format(DeployArgumentsPattern, files[0].FullName), () => { pLaunch.Start(); });
                //System.Diagnostics.Process pDeploy = CreateDeployProcess(files[0].FullName, () => { pLaunch.Start(); });
                if (pDeploy.Start())
                {
                    UnityEngine.Debug.Log("ADB deployment started..");
                    return true;
                }
            }
            return false;
            */
        }

        public async Task<bool> DeployAsync(Action callback = null)
        {
            DirectoryInfo diOutputPathGradle = new DirectoryInfo(OutputPathGradle);
            FileInfo fiGradleBuildFile = diOutputPathGradle.GetFiles(GradleBuildFileName, SearchOption.AllDirectories).FirstOrDefault();

            if (fiGradleBuildFile != null)
            {
                string gradleOutputPath = Path.Combine(fiGradleBuildFile.Directory.FullName, "build", "outputs", "apk"); // last is configuration and 
                DirectoryInfo diGradleOutputPath = new DirectoryInfo(gradleOutputPath);

                if (diGradleOutputPath.Exists)
                {
                    FileInfo[] files = diGradleOutputPath.GetFiles("*.apk", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        gradleOutputPath = files[0].FullName;

                        System.Diagnostics.Process pDeploy = BuildBridgeUtilities.CreateProcess(string.Format(DeployCommandPattern, PathADB), string.Format(DeployArgumentsPattern, files[0].FullName));
                        pDeploy.StartInfo.UseShellExecute = false;
                        pDeploy.StartInfo.RedirectStandardOutput = true;

                        UnityEngine.Debug.Log("APK deployment to device started...");
                        await Task.Delay(50);
                        if (pDeploy.Start())
                        {
                            bool deployFinished = await Task.FromResult(pDeploy.WaitForExit(30000));
                            if (!deployFinished)
                            {
                                pDeploy.Kill();
                                UnityEngine.Debug.LogWarning("APK deployed to device timed out!");
                                return false;
                            }

                            UnityEngine.Debug.Log(pDeploy.StandardOutput.ReadToEnd());
                            UnityEngine.Debug.Log("APK deployment to device finished!");
                            await Task.Delay(50);

                        }

                        System.Diagnostics.Process pLaunch = BuildBridgeUtilities.CreateProcess(string.Format(LaunchCommandPattern, PathADB), string.Format(LaunchArgumentsPattern, BuildBridgeAndroid.AppIdentifier));
                        pLaunch.StartInfo.UseShellExecute = false;
                        pLaunch.StartInfo.RedirectStandardOutput = true;

                        UnityEngine.Debug.Log("ADB launch application on device started...");
                        await Task.Delay(50);
                        if (pLaunch.Start())
                        {
                            bool launchFinished = await Task.FromResult(pLaunch.WaitForExit(30000));
                            if (!launchFinished)
                            {
                                pLaunch.Kill();
                                UnityEngine.Debug.LogWarning("ADB launch application on device timed out!");
                                return false;
                            }
                            UnityEngine.Debug.Log(pLaunch.StandardOutput.ReadToEnd());
                            UnityEngine.Debug.Log("ADB launch application on device finished!");
                            await Task.Delay(50);

                        }
                        if (callback != null)
                        {
                            callback.Invoke();
                        }
                        return true;
                    }
                    UnityEngine.Debug.LogWarning("APK deployment failed. APK file not found.");
                }
                UnityEngine.Debug.LogWarning("APK deployment failed. Gradle build output directory does not exist.");
            }
            UnityEngine.Debug.LogWarning("APK deployment failed. Gradle project directory does not exist.");
            return false;
        }


        public override bool OpenLocation()
        {
            string path = BuildBridgeAndroid.PathProject;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            string pathToOpen = files.Length > 0 ? files[0].FullName : path;

            if (Process.Start("explorer", "/select," + path) != null)
                return true;
            return false;
        }
        public override bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback = null, Action buildCallback = null, Action deployCallback = null)
        {
            // put together the last callback (invoking deploy step)
            //Action buildStepCallback = () =>
            //{
            //    if ((steps & IBuildBridgeSteps.Deploy) == IBuildBridgeSteps.Deploy)
            //    {
            //        UnityEngine.Debug.Log("Deploy...");
            //        this.Deploy(deployCallback);
            //    }
            //    if (buildCallback != null) buildCallback.Invoke();
            //};
            //// put together the second callback (invoking build step)
            //Action generateStepCallback = () =>
            //{
            //    if ((steps & IBuildBridgeSteps.Build) == IBuildBridgeSteps.Build)
            //        this.Build(args, buildStepCallback);
            //    if (generateCallback != null) generateCallback.Invoke();
            //};
            //// put together the initial callback (invoking generate step)
            //Action callback = () =>
            //{
            //    if ((steps & IBuildBridgeSteps.Generate) == IBuildBridgeSteps.Generate)
            //        this.Generate(options, generateStepCallback);
            //};
            //callback.Invoke();

            // reimplement as async methods


            return true;
        }

        private async Task<bool> BuildStepsAsync(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback = null, Action buildCallback = null, Action deployCallback = null)
        {
            bool resultGenerate = false;
            bool resultBuild = false;
            bool resultDeploy = false;

            if ((steps & IBuildBridgeSteps.Generate) == IBuildBridgeSteps.Generate)
                resultGenerate = this.Generate(options, generateCallback);
            else
                resultGenerate = true;

            if ((steps & IBuildBridgeSteps.Build) == IBuildBridgeSteps.Build)
            {
                if (resultGenerate)
                    resultBuild = await Task.FromResult(this.Build(args, buildCallback));
            }
            else
                resultBuild = true;

            if ((steps & IBuildBridgeSteps.Deploy) == IBuildBridgeSteps.Deploy)
            {
                if (resultBuild)
                    resultDeploy = await Task.FromResult(this.Deploy(deployCallback));
            }
            else
                resultDeploy = true;

            return resultGenerate & resultBuild & resultDeploy;
        }

        public override bool VerifyToolchain()
        {
            // check for android SDK and gradle installation path
            return base.VerifyToolchain();
        }


        private static void Prepare()
        {
            string prepareMessage = "Preparing Android Unity Build Bridge Step.." + Environment.NewLine
                + "ProjectPath: " + PathProject + Environment.NewLine
                + "OutputPathGradle: " + OutputPathGradle + Environment.NewLine
                + "OutputNameAPK: " + OutputNameAPK + Environment.NewLine
                + "AppIdentifier: " + AppIdentifier + Environment.NewLine
                + "IsBatchMode: " + IsBatchMode;
            UnityEngine.Debug.Log(prepareMessage);
        }

        private class AndroidBuildSettings
        {
            public string KeyAliasName
            { get; private set; }
            public string KeyAliasPass
            { get; private set; }
            public AndroidBuildSystem BuildSystem
            { get; private set; }
            public bool ExportProject
            { get; private set; }


            public static AndroidBuildSettings StoreSettings()
            {
                return new AndroidBuildSettings
                {
                    KeyAliasName = PlayerSettings.Android.keyaliasName,
                    KeyAliasPass = PlayerSettings.Android.keyaliasPass,
                    BuildSystem = EditorUserBuildSettings.androidBuildSystem,
                    ExportProject = EditorUserBuildSettings.exportAsGoogleAndroidProject
                };
            }

            public static void ApplySettings(AndroidBuildSettings settings)
            {
                PlayerSettings.Android.keyaliasName = settings.KeyAliasName;
                PlayerSettings.Android.keyaliasPass = settings.KeyAliasPass;
                EditorUserBuildSettings.androidBuildSystem = settings.BuildSystem;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = settings.ExportProject;
            }
            public static void ApplySettingsForGradleProject()
            {
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            }

        }

        public class Preferences
        {
            [PreferenceItem("Unity Build Bridge/Android")]
            public static void PreferencesGUI()
            {
                // compiler options
                EditorGUILayout.HelpBox("These settings & options are specifically for the Android build pipeline based on gradle on a windows system.", MessageType.None, true);

            }

        }

    }

}