using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_2018_1_OR_NEWER
#endif


namespace VRTX.Build
{
    public class BuildBridgeIOS : BuildBridgeBase<BuildBridgeIOS>
    {
        //private const string BuildArgs_Default = "-multicore -ipa -archs \"arm64\" -cleanmodules -deploy -xcname \"Unity-iPhone\" ";

        private const string OTADeploy = "Toolchain/deployota.exe";

        private static string Path_IOS_Packages
        { get { return Path.Combine(OutputPathXCode, "Packages"); } }

        private static string Path_BuildEnv_BuildCMD
        { get { return Path.Combine(BuildBridgeIOS.Preferences.EnvironmentPath, "build.cmd"); } }
        private static string Path_BuildEnv_OTADeploy
        { get { return Path.Combine(BuildBridgeIOS.Preferences.EnvironmentPath, "Toolchain\\ideployota.exe"); } }


        private static string _outputPathXCode = string.Empty;

        public static string OutputPathXCode
        {
            get
            {
                if (_diProject == null)
                    _diProject = new DirectoryInfo(UnityEngine.Application.dataPath).Parent;
                if (!_diProject.Exists)
                    _diProject.Create();
                if (_outputPathXCode.Equals(string.Empty))
                    _outputPathXCode = Path.Combine(_diProject.FullName, "Xcode");
                return _outputPathXCode;
            }
        }


#if UNITY_EDITOR
        [MenuItem(BuildBridgeMenu.MenuBase + "Generate (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 1)]
        public new static void BuildBridgeGenerate()
        { Instance.Generate(BuildOptions.None, null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Build (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 2)]
        public new static void BuildBridgeBuild()
        { Instance.Build(BuildBridgeIOS.Preferences.BuildArgumentsIOS, null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Deploy (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 3)]
        public new static void BuildBridgeDeploy()
        { Instance.Deploy(null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Generate, Build and Deploy (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 4)]
        public new static void GenerateBuildAndDeploy()
        { Instance.BuildSteps(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build | IBuildBridgeSteps.Deploy, BuildOptions.None, BuildBridgeIOS.Preferences.BuildArgumentsIOS, null, null, null); }

        public new static void GenerateAndBuild()
        { Instance.BuildSteps(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build, BuildOptions.None, BuildBridgeIOS.Preferences.BuildArgumentsIOS, null, null, null); }
#endif


        public override bool Generate(BuildOptions options, Action callback)
        {
            BuildReport report = null;
            bool result = BuildBridgeUtilities.BuildSourceWithReport(BuildBridgeIOS.OutputPathXCode, BuildTarget.iOS, options, out report);
            if (result)
            {
                UnityEngine.Debug.Log("Build succeeded: " + report.summary.outputPath);
                if (callback != null) callback.Invoke();
            }
            return result;
        }


        public override bool Build(string args, Action callback)
        {
            string path = BuildBridgeIOS.OutputPathXCode;
            Process p = new Process();
            UnityEngine.Debug.Log(Path_BuildEnv_BuildCMD + " " + "\"" + path + "\" " + args);
            p.StartInfo = new ProcessStartInfo(Path_BuildEnv_BuildCMD, " " + "\"" + path + "\" " + args);
            if (p.Start())
            {
                UnityEngine.Debug.Log("iOS Build started..");
                if (callback != null) callback.Invoke();
                return true;
            }
            return false;
            // "%USERPROFILE%\iOS Build Environment\build.cmd" C:\Development\OpenCV\XCode -multicore -ipa -archs "arm64"
        }
        public override bool Deploy(Action callback)
        {
            return this.Deploy(false, callback);
        }
        private bool Deploy(bool multiple, Action callback)
        {
            //  multiple";
            string path = BuildBridgeIOS.Path_IOS_Packages;

            if (!Directory.Exists(path))
                UnityEngine.Debug.LogWarning("The project seems not to be build yet. Please generate the project for the iOS target platform and build it with the iOS Build Environment.");
            else
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] ipaFiles = di.GetFiles("*.ipa");
                if (ipaFiles.Length > 0)
                {
                    Process p = BuildBridgeUtilities.CreateProcess(Path_BuildEnv_OTADeploy, "\"" + ipaFiles[0].FullName + "\"");
                    p.StartInfo.CreateNoWindow = false;
                    // append "multiple" parameter to provide multiple OTA deployments at once
                    if (multiple)
                        p.StartInfo.Arguments += " multiple";

                    if (p.Start())
                    {
                        UnityEngine.Debug.Log("OTA deployment started..");
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool OpenLocation()
        {
            string path = BuildBridgeIOS.Path_IOS_Packages;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            string pathToOpen = files.Length > 0 ? files[0].FullName : path;

            if (Process.Start("explorer", "/select," + path) != null)
                return true;
            return false;
        }
        public override bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback, Action buildCallback, Action deployCallback)
        {
            // put together the last callback (invoking deploy step)
            Action buildStepCallback = () =>
            {
                if ((steps & IBuildBridgeSteps.Deploy) == IBuildBridgeSteps.Deploy)
                {
                    UnityEngine.Debug.Log("Deploy...");
                    this.Deploy(deployCallback);
                }
                if (buildCallback != null) buildCallback.Invoke();
            };
            // put together the second callback (invoking build step)
            Action generateStepCallback = () =>
            {
                if ((steps & IBuildBridgeSteps.Build) == IBuildBridgeSteps.Build)
                    this.Build(args, buildStepCallback);
                if (generateCallback != null) generateCallback.Invoke();
            };
            // put together the initial callback (invoking generate step)
            Action callback = () =>
            {
                if ((steps & IBuildBridgeSteps.Generate) == IBuildBridgeSteps.Generate)
                    this.Generate(options, generateStepCallback);
            };

            callback.Invoke();

            return true;
        }

        public override bool VerifyToolchain()
        {
            // check for iOS Build environment path and existance of build.cmd script file
            return base.VerifyToolchain();
        }

        private static void Prepare()
        {
            string prepareMessage = "Preparing iOS Unity Build Bridge Step.." + Environment.NewLine
                + "OutputPathXcode: " + OutputPathXCode + Environment.NewLine
                + "AppIdentifier: " + AppIdentifier;
            UnityEngine.Debug.Log(prepareMessage);
        }

        public class Preferences
        {
            [PreferenceItem("Unity Build Bridge/iOS")]
            public static void PreferencesGUI()
            {
                // compiler options
                EditorGUILayout.HelpBox("These settings & options are specifically for the use of the iOS Build Environment on a windows system.", MessageType.None, true);
                foreach (PreferenceUIOption.Base option in _optionsIOS.Values)
                    option.DrawUI();
            }

            // constant fields (should not change between versions)
            private const string PKey_EnvironmentPath = "VRTX.BuildBridge.iOS.EnvironmentPath";
            private const string PKey_iOSOptionCleanCompilerCache = "VRTX.BuildBridge.iOS.CleanCompilerCache";
            private const string PKey_iOSOptionCompileWithMultipleCores = "VRTX.BuildBridge.iOS.CompileWithMultipleCores";
            private const string PKey_iOSOptionBuildArchitecture64 = "VRTX.BuildBridge.iOS.BuildArchitecture64";
            private const string PKey_iOSOptionBuildArchitecture32 = "VRTX.BuildBridge.iOS.BuildArchitecture32";
            private const string PKey_iOSOptionDeployAfterBuild = "VRTX.BuildBridge.iOS.DeployAfterBuild";

            private const string BuildEnvName = "iOS Build Environment";


            protected static Dictionary<string, PreferenceUIOption.Base> _optionsIOS = new Dictionary<string, PreferenceUIOption.Base>
            {
                { "Settings (iOS)", new PreferenceUIOption.Title("Settings (iOS)", "") },
                { PKey_EnvironmentPath, new PreferenceUIOption.Folder(PKey_EnvironmentPath, Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), BuildEnvName),
                    "Path", "Path to your local iOS build Environment installation. This is different when installed from Unity Asset Store package.",
                    "Select path to iOS Build Environment", "%USERPROFILE%") },
                { "Options (iOS)", new PreferenceUIOption.Title("Options (iOS)", "") },
                { PKey_iOSOptionCleanCompilerCache, new PreferenceUIOption.Bool(PKey_iOSOptionCleanCompilerCache, true, "Clean compiler cache", "Clean the compiler cache and force complete rebuild before building the Xcode project") },
                { PKey_iOSOptionCompileWithMultipleCores, new PreferenceUIOption.Bool(PKey_iOSOptionCompileWithMultipleCores, true, "Use multiple cores", "Allow the compiler to use all available cores on this machine") },
                { PKey_iOSOptionBuildArchitecture64, new PreferenceUIOption.Bool(PKey_iOSOptionBuildArchitecture64, true, "Build 64-bit", "Build the 64-bit version of the application bundle") },
                //{ PKey_iOSOptionBuildArchitecture32, new PreferenceUIOptions.Bool(PKey_iOSOptionBuildArchitecture32, false, "Build 32-bit", "Build the 32-bit version of the application bundle") },
                { PKey_iOSOptionDeployAfterBuild, new PreferenceUIOption.Bool(PKey_iOSOptionDeployAfterBuild, false, "Deploy OTA", "Invoke the over-the-air deployment of the application bundle") }
            };


            public static string EnvironmentPath
            { get { return ((PreferenceUIOption.Folder)_optionsIOS[PKey_EnvironmentPath]).Value; } }

            public static string BuildArgumentsIOS
            {
                get
                {
                    string buildArgs = "";
                    buildArgs += " -ipa -xcname \"Unity-iPhone\"";

                    if ((_optionsIOS[PKey_iOSOptionCompileWithMultipleCores] as PreferenceUIOption.Bool).Value)
                        buildArgs += " -multicore";
                    if ((_optionsIOS[PKey_iOSOptionCleanCompilerCache] as PreferenceUIOption.Bool).Value)
                        buildArgs += " -cleanmodules -rebuild";
                    if ((_optionsIOS[PKey_iOSOptionBuildArchitecture64] as PreferenceUIOption.Bool).Value)
                        buildArgs += " -archs \"arm64\"";
                    //if ((_optionsIOS[PKey_iOSOptionBuildArchitecture32] as PreferenceUIOptions.Bool).Value)
                    //    buildArgs += " -archs \"\"";
                    if ((_optionsIOS[PKey_iOSOptionDeployAfterBuild] as PreferenceUIOption.Bool).Value)
                        buildArgs += " -deploy";

                    buildArgs = buildArgs.Trim();
                    return buildArgs;
                }
            }

        }

    }

}