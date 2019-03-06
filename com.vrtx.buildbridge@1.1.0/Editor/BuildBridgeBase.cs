using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace VRTX.Build
{
    public enum IBuildBridgeSteps
    {
        Custom = 0,
        Generate = 1,
        Build = 2,
        Deploy = 4,
    }

    public enum BuildBridgeMethods
    {
        BuildBridgeGenerate,
        BuildBridgeBuild,
        BuildBridgeDeploy,
        GenerateBuildAndDeploy
    }

    internal interface IBuildBridge
    {
        bool Generate(BuildOptions options, Action callback);
        bool Build(string args, Action callback);
        bool Deploy(Action callback);
        bool OpenLocation();
        bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback, Action buildCallback, Action deployCallback);
    }

    public abstract class BuildBridgeBase<T> : SingletonBase<T>, IBuildBridge where T : class
    {

        public static void BuildBridgeGenerate()
        { }
        public static void BuildBridgeBuild()
        { }
        public static void BuildBridgeDeploy()
        { }
        public static void GenerateBuildAndDeploy()
        { }


        public abstract bool Generate(BuildOptions options, Action callback);
        public abstract bool Build(string args, Action callback);
        public abstract bool Deploy(Action callback);

        public abstract bool OpenLocation();
        public abstract bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback, Action buildCallback, Action deployCallback);


        private static string GetPlatformIdentifierLI()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return string.Empty;
#endif
        }
        private static Type CurrentBuildBridge()
        {
            Type TBuildBridgeBase = typeof(BuildBridgeBase<T>).GetGenericTypeDefinition();
            IEnumerable<Type> types = TBuildBridgeBase.Assembly.GetTypes().Where(x =>
            { return x != null && x.BaseType != null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == TBuildBridgeBase; });

            string platformID = GetPlatformIdentifierLI();
            if (!String.IsNullOrEmpty(platformID))
                return types.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains(platformID));
            return null;
        }
        internal static bool CallStaticMethod(string methodName)
        {
            Type tCurrentBuildBridge = CurrentBuildBridge();
            if (tCurrentBuildBridge != null)
            {
                MethodInfo info = tCurrentBuildBridge.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (info != null)
                {
                    info.Invoke(null, null);
                    return true;
                }
            }
            return false;
        }

    }

    internal class BuildBridgeProxy : BuildBridgeBase<BuildBridgeProxy>
    {
        public override bool Build(string args, Action callback)
        { return false; }
        public override bool BuildSteps(IBuildBridgeSteps steps, BuildOptions options, string args, Action generateCallback, Action buildCallback, Action deployCallback)
        { return false; }
        public override bool Deploy(Action callback)
        { return false; }
        public override bool Generate(BuildOptions options, Action callback)
        { return false; }
        public override bool OpenLocation()
        { return false; }
    }

    /// <summary>
    /// A base class for the singleton design pattern.
    /// </summary>
    /// <typeparam name="T">Class type of the singleton</typeparam>
    public abstract class SingletonBase<T> where T : class
    {
        /// <summary>
        /// Static instance. Needs to use lambda expression
        /// to construct an instance (since constructor is private).
        /// </summary>
#if NET_4_6 || NET_STANDARD_2_0
        private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());
        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance { get { return sInstance.Value; } }
#else
        private static readonly T sInstance = CreateInstanceOfT();
        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance { get { return sInstance; } }
#endif

        protected SingletonBase()
        { }

        /// <summary>
        /// Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns></returns>
        private static T CreateInstanceOfT()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }

    }

}