using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace ET {

    public class CodeLoader: Singleton<CodeLoader> {
        private Assembly model;

        public void Start() {
            if (Define.EnableCodes) { // 这个变量，大概是方便 Unity 的编辑器模式下的使用引用程序集，成为可能。前提：【双端模式】? 对吗？【TODO】：
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                if (globalConfig.CodeMode != CodeMode.ClientServer) { // 不是双端模式：抛异常
                    throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
                }
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
                EventSystem.Instance.Add(types);
                foreach (Assembly ass in assemblies) {
                    string name = ass.GetName().Name;
                    if (name == "Unity.Model.Codes") {
                        this.model = ass;
                    }
                }
            }
            else {
                byte[] assBytes;
                byte[] pdbBytes;
                if (!Define.IsEditor) {
                    Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
                    assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;
                    pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;
                    if (Define.EnableIL2CPP) {
                        HybridCLRHelper.Load();
                    }
                } else {
                    assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
                    pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
                }
                this.model = Assembly.Load(assBytes, pdbBytes);
                this.LoadHotfix();
            }
            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start"); // 赋值：这里成为程序入口
            start.Run();
        }
        // 热重载调用该方法
        public void LoadHotfix() {
            byte[] assBytes;
            byte[] pdbBytes;
            if (!Define.IsEditor) {
                Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
                assBytes = ((TextAsset)dictionary["Hotfix.dll"]).bytes;
                pdbBytes = ((TextAsset)dictionary["Hotfix.pdb"]).bytes;
            }
            else {
                // 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
                string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
                if (logicFiles.Length != 1) {
                    throw new Exception("Logic dll count != 1");
                }
                string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
                assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
                pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));
            }
            Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
            
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.model, hotfixAssembly);
            
            EventSystem.Instance.Add(types);
        }
    }
}