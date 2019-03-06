using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace VRTX.Build
{
    public class BuildBridgeAndroid : BuildBridgeBase<BuildBridgeAndroid>
    {
        private const string DeployCommandPattern = "\"{0}\""; // insert PathADB value
        private const string DeployArgumentsPattern = "install -r " + "\"{0}\""; // insert path-to-APK value

        private const string LaunchCommandPattern = "\"{0}\""; // insert PathADB value
        private const string LaunchArgumentsPattern = "shell monkey -p {0} -c android.intent.category.LAUNCHER 1"; // insert PlayerSettings.applicationIdentifier value



        private static DirectoryInfo _diProject = null;
        private static string _pathAPK = string.Empty;
        private static string _nameAPK = string.Empty;

        public static string PathAPK
        {
            get
            {
                if (_diProject == null)
                    _diProject = new DirectoryInfo(UnityEngine.Application.dataPath).Parent;
                if (!_diProject.Exists)
                    _diProject.Create();
                if (_pathAPK.Equals(string.Empty))
                    _pathAPK = _diProject.FullName;
                return _pathAPK;
            }
        }
        public static string NameAPK
        {
            get
            {
                if (String.IsNullOrEmpty(_nameAPK))
                    _nameAPK = PlayerSettings.productName + ".apk";
                return _nameAPK;
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
        { /* Instance.Build("", null); */ }

        [MenuItem(BuildBridgeMenu.MenuBase + "Deploy (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 3)]
        public new static void BuildBridgeDeploy()
        { Instance.Deploy(null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Generate, Build and Deploy (Android)", priority = BuildBridgeMenu.PriorityBasePlatforms + 4)]
        public new static void GenerateBuildAndDeploy()
        { }
#endif

        private static void DeployProcess_Exited(object sender, System.EventArgs e)
        {
            UnityEngine.Debug.Log("ADB Deploy finished!");
        }
        private static void LaunchProcess_Exited(object sender, System.EventArgs e)
        {
            UnityEngine.Debug.Log("ADB Launch finished!");
        }


        public override bool Generate(BuildOptions options, Action callback)
        {
            string[] prevValues = new string[] { PlayerSettings.Android.keyaliasName, PlayerSettings.Android.keyaliasPass };
            // reset keyAlias to create debug apk
            PlayerSettings.Android.keyaliasName = string.Empty;
            PlayerSettings.Android.keyaliasPass = string.Empty;
            // run project generation
            bool result = BuildBridgeUtilities.BuildSource(Path.Combine(BuildBridgeAndroid.PathAPK, BuildBridgeAndroid.NameAPK), BuildTarget.Android, options);
            // restore previous keyAlias settings
            PlayerSettings.Android.keyaliasName = prevValues[0];
            PlayerSettings.Android.keyaliasPass = prevValues[1];

            return result;
        }
        public override bool Build(string args, Action callback)
        {
            return false;
        }
        public override bool Deploy(Action callback)
        {
            string apkPath = PathAPK;

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
        }
        public override bool OpenLocation()
        {
            string path = BuildBridgeAndroid.PathAPK;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            string pathToOpen = files.Length > 0 ? files[0].FullName : path;

            if (Process.Start("explorer", "/select," + path) != null)
                return true;
            return false;
        }
        public override bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback, Action buildCallback, Action deployCallback)
        {
            // ! ! ! !
            // elaborate: use the callbacks to include the next steps into the invokation chain
            //if ((steps & IBuildBridgeSteps.Generate) == IBuildBridgeSteps.Generate)
            //    this.Generate(options, null);

            //if ((steps & IBuildBridgeSteps.Build) == IBuildBridgeSteps.Build)
            //    this.Build(args, null);

            //if ((steps & IBuildBridgeSteps.Deploy) == IBuildBridgeSteps.Deploy)
            //    this.Deploy(null);
            return false;

        }
    }

}