using System;
using UnityEditor;
using UnityEngine;
namespace VRTX.Build
{
    public class BuildBridgeMenu
    {
        public const string HotKeyModifier = "#&"; // Shift + Alt
        public const string MenuBase = "Window/Build/";
        public const int PriorityBase = 5000;
        public const int PriorityBasePlatforms = PriorityBase + 50;


        private const string CurrentPlatformID =
#if UNITY_IOS
        "(iOS)";
#elif UNITY_ANDROID
        "(Android)";
#else
        "";
#endif

        #region Separator Entry (Current Platform)
        [MenuItem(BuildBridgeMenu.MenuBase + "===== Current Platform " + CurrentPlatformID + " =====", priority = BuildBridgeMenu.PriorityBase + 0)]
        public static void SpacerCurrentPlatform()
        { }
        [MenuItem(BuildBridgeMenu.MenuBase + "===== Current Platform " + CurrentPlatformID + " =====", true)]
        public static bool ValidateCurrentPlatform()
        { return false; }
        #endregion


#if UNITY_EDITOR
        [MenuItem(BuildBridgeMenu.MenuBase + "Generate " + BuildBridgeMenu.HotKeyModifier + "G", priority = BuildBridgeMenu.PriorityBase + 1)]
        public static void CurrentGenerate()
        { CallOnCurrentImplementation(BuildBridgeMethods.BuildBridgeGenerate); }
        [MenuItem(BuildBridgeMenu.MenuBase + "Build " + BuildBridgeMenu.HotKeyModifier + "B", priority = BuildBridgeMenu.PriorityBase + 2)]
        public static void CurrentBuild()
        { CallOnCurrentImplementation(BuildBridgeMethods.BuildBridgeBuild); }
        [MenuItem(BuildBridgeMenu.MenuBase + "Deploy " + BuildBridgeMenu.HotKeyModifier + "D", priority = BuildBridgeMenu.PriorityBase + 3)]
        public static void CurrentDeploy()
        { CallOnCurrentImplementation(BuildBridgeMethods.BuildBridgeDeploy); }
        [MenuItem(BuildBridgeMenu.MenuBase + "Generate, Build and Deploy " + BuildBridgeMenu.HotKeyModifier + "F", priority = BuildBridgeMenu.PriorityBase + 4)]
        public static void CurrentGenerateBuildAndDeploy()
        { CallOnCurrentImplementation(BuildBridgeMethods.GenerateBuildAndDeploy); }
#endif

        private static void CallOnCurrentImplementation(BuildBridgeMethods method)
        {
            if (!BuildBridgeProxy.CallStaticMethod(method.ToString()))
                Debug.LogWarning(String.Format("Failed to call '{0}'. Most likely no implementation for the current platform exists. Check your target platform and build bridge implementations.", method));
        }
    }

}