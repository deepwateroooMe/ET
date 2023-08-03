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
                if (assembly.GetName().Name == "Model") { // 现在几个域，里面的文件，还是弄得清楚的。。。
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
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true); // 赋值更新前：先把先前非空引用释放掉，并手动调GC清理垃圾，释放内存空间
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll"); // 读文件：将两个文件读入内存
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes)); // 转化为内存流，内存流上加载程序集
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(Assembly.GetEntryAssembly(), typeof(Init).Assembly, typeof (Game).Assembly, this.model, hotfixAssembly);
            // 【心脏：事件系统的扫描管理】
            EventSystem.Instance.Add(types); // <<<<<<<<<<<<<<<<<<<< 
        }
    }
}