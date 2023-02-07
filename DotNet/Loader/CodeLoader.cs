using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
    public class CodeLoader: Singleton<CodeLoader>
    {
        private AssemblyLoadContext assemblyLoadContext;

        private Assembly model;

        public void Start()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == "Model")
                {
// 它只选定了一个,[客户端应用的热更新程序域?还是ET框架的服务端热更新程序域 ?],其它的不管
                    this.model = assembly;
                    break;
                }
            }
            this.LoadHotfix();
            
            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");  // 方法是固定的
            start.Run();
        }

        public void LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true); // 这里是客户端热更新程序域
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));

            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(Assembly.GetEntryAssembly(), typeof(Init).Assembly, typeof (Game).Assembly, this.model, hotfixAssembly);
			
            EventSystem.Instance.Add(types);
        }
    }
}