using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace ET {
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake {
        private AssemblyLoadContext assemblyLoadContext;
        private Assembly assembly;
        public void Awake() {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies) {
                if (ass.GetName().Name == "Model") {
                    this.assembly = ass;
                    break;
                }
            }
            Assembly hotfixAssembly = this.LoadHotfix(); // 【TODO】：记得一两年前？亲爱的表哥的活宝妹第一次运行这个框架，这个加载热更新域，还有个什么机关？
			// 【TODO】：这里加的，为什么是这几个程序域？CodeLoader, Hotfix, Model,【world 相关】
			// 【world 相关】：World详解，几种Singleton,线程安全的思考(ReloadDl ReloadConfig)，找到临界区【TODO】：这里像是还有点儿知识点，改天弄懂
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });
            IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start"); // CodeTypes 帮助加载双端
            start.Run();
        }
        private Assembly LoadHotfix() {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            return hotfixAssembly;
        }
        public void Reload() {
            Assembly hotfixAssembly = this.LoadHotfix();
            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });
            codeTypes.CreateCode();
            Log.Debug($"reload dll finish!");
        }
    }
}