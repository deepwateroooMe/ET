using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace ET {

    public class CodeLoader: Singleton<CodeLoader> {
        private AssemblyLoadContext assemblyLoadContext;
        private Assembly model;
        public void Start() {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                if (assembly.GetName().Name == "Model") {
                    this.model = assembly;
                    break;
                }
            }
            this.LoadHotfix(); // 先加载：热更新域: 先加载这个，不会找不到，某些变量或是类吗？好好看下，两个域Model Hotfix 加载顺序与细节
            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start"); // 再Model 域：
            start.Run();
        }
        public void LoadHotfix() {
            assemblyLoadContext?.Unload();
            GC.Collect(); // 手动调用：【服务端】启动时，主动手动、清理一次垃圾，主要是想要，释放资源，提升性能
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
			// 重点：【服务端】程序域加载，扫描域里的各种标签系【框架里的各种标签封装、扫描】，支持ET 框架里的【事件系统】
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(Assembly.GetEntryAssembly(), typeof(Init).Assembly, typeof (Game).Assembly, this.model, hotfixAssembly);
            EventSystem.Instance.Add(types);
        }
    }
}