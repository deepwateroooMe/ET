using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ET.Client {
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, byte[]>> {
        public override Dictionary<Type, byte[]> Handle(ConfigComponent.GetAllConfigBytes args) {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute));
            if (Define.IsEditor) { // 【编辑器模式下】：
                string ct = "cs";
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig"); // 加载全局模式：这里没有看懂
                CodeMode codeMode = globalConfig.CodeMode;
                switch (codeMode) {
                    case CodeMode.Client:
                        ct = "c";
                        break;
                    case CodeMode.Server:
                        ct = "s";
                        break;
                    case CodeMode.ClientServer:
                        ct = "cs";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                List<string> startConfigs = new List<string>() {
                    "StartMachineConfigCategory", 
                    "StartProcessConfigCategory", 
                    "StartSceneConfigCategory", 
                    "StartZoneConfigCategory",
                };
                foreach (Type configType in configTypes) {
                    string configFilePath;
                    if (startConfigs.Contains(configType.Name)) {
                        configFilePath = $"../Config/Excel/{ct}/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                    } else {
                        configFilePath = $"../Config/Excel/{ct}/{configType.Name}.bytes";
                    }
                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            } else { // 这个分支：先花点儿时间，走一遍真正的【客户端】（而非编辑器模式）从资源包读配置的过程，从哪里下载的资源包，以及找个本地资源包出来看下，内容应该是一样的？【这里还，不曾涉及，要从哪个资源服务器下载热更新资源包的过程】
                using (Root.Instance.Scene.AddComponent<ResourcesComponent>()) { // 这里只是： using 结束后，自动回收垃圾
                    const string configBundleName = "config.unity3d";
                    ResourcesComponent.Instance.LoadBundle(configBundleName);

                    foreach (Type configType in configTypes) {
                        TextAsset v = ResourcesComponent.Instance.GetAsset(configBundleName, configType.Name) as TextAsset;
                        output[configType] = v.bytes;
                    }
                }
            }
            return output;
        }
    }
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigComponent.GetOneConfigBytes, byte[]> {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args) {
            // TextAsset v = ResourcesComponent.Instance.GetAsset("config.unity3d", configName) as TextAsset;
            // return v.bytes;
            throw new NotImplementedException("client cant use LoadOneConfig");
        }
    }
}