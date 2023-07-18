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
                if (assembly.GetName().Name == "Model") { // 不是搞不懂程序域么，可以倒着去追，去找打包时，哪些部分的源码被打进Model 里了。。
                    this.model = assembly;
                    break;
                }
            }
            this.LoadHotfix();
            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
            start.Run();
        }
        public void LoadHotfix() {
            assemblyLoadContext?.Unload();
            GC.Collect(); // 【自己管理】：手动调用一次垃圾回收
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(Assembly.GetEntryAssembly(), typeof(Init).Assembly, typeof (Game).Assembly, this.model, hotfixAssembly);
            
            EventSystem.Instance.Add(types);
        }
    }
}
