using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
#if UNITY_2018_1_OR_NEWER
#endif


namespace VRTX.Build
{
    public class BuildBridgeIOS : BuildBridgeBase<BuildBridgeIOS>
    {
        private const string OTADeploy = "Toolchain/deployota.exe";

        private static string Path_IOS_Packages
        { get { return Path.Combine(OutputPathXCode, "Packages"); } }

        private static string Path_BuildEnv_BuildCMD
        { get { return Path.Combine(BuildBridgePreferences.EnvironmentPath, "build.cmd"); } }
        private static string Path_BuildEnv_OTADeploy
        { get { return Path.Combine(BuildBridgePreferences.EnvironmentPath, "Toolchain\\ideployota.exe"); } }


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


        private const string BuildArgs_Default = "-multicore -ipa -archs \"arm64\" -cleanmodules -deploy -xcname \"Unity-iPhone\" ";

#if UNITY_EDITOR
        [MenuItem(BuildBridgeMenu.MenuBase + "Generate (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 1)]
        public new static void BuildBridgeGenerate()
        { Instance.Generate(BuildOptions.None, null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Build (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 2)]
        public new static void BuildBridgeBuild()
        { Instance.Build(BuildBridgeIOS.BuildArgs_Default, null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Deploy (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 3)]
        public new static void BuildBridgeDeploy()
        { Instance.Deploy(null); }

        [MenuItem(BuildBridgeMenu.MenuBase + "Generate, Build and Deploy (iOS)", priority = BuildBridgeMenu.PriorityBasePlatforms + 4)]
        public new static void GenerateBuildAndDeploy()
        { Instance.BuildSteps(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build | IBuildBridgeSteps.Deploy, BuildOptions.None, "", null, null, null); }

        public new static void GenerateAndBuild()
        { Instance.BuildSteps(IBuildBridgeSteps.Generate | IBuildBridgeSteps.Build, BuildOptions.None, "", null, null, null); }
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
            p.StartInfo = new ProcessStartInfo(Path_BuildEnv_BuildCMD, "\"" + path + "\" " + args);
            p.EnableRaisingEvents = true;

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

    }

}