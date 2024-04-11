using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ET.Client {
    [Invoke] // ConfigComponent 的【客户端】实现逻辑
	// 【双端、都】CodeLoader==>Entry.Start()==>LoadAsync()==>ConfigComponent 
	// 【客户端】：Unity/Assets/Config/Excel/... 里，也是有着客户端的配置 excel 相关文件，启动时是会【扫这些配置文件的【TODO】：去找，什么时候扫的？】
	// 【客户端】：虽然是【客户端】，但是为了与游戏服务器交互，客户端的路径目录里，是存有必要的、如网关服的配置信息的。【TODO】：确认细节
	// 客户端可以拿这些必要信息，与网关服通信，建立与服务端的交互【这后半句写得不一定对】
	// 【问题：客户端是什么时候，获得这些服务端的配置文件Excel的？客户端何以知晓？如何知道和获得的？】
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, byte[]>> {
        public override Dictionary<Type, byte[]> Handle(ConfigComponent.GetAllConfigBytes args) {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute)); // 程序域加载时，系统性一次性全扫过、所有 BaseAttribute 的继承类
            if (Define.IsEditor) { // 编辑器模式下：配置的路径地址
                string ct = "cs";
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
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
                    }
                    else {
                        configFilePath = $"../Config/Excel/{ct}/{configType.Name}.bytes";
                    }
                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            } else { // 非编辑器模式下的客户端：
                using (Root.Instance.Scene.AddComponent<ResourcesComponent>()) {
                    const string configBundleName = "config.unity3d"; // 客户端从这个热更新资源包，来读配置信息；去服务端找：哪里打包了相关配置？【TODO】：
					// 去拿和读：热更新资源服里、服务端？的配置信息？【TODO】：
					// 加载资源包：细节是，支扫和拿所有必须依赖包，再不出异常地加载，当前需要的配置资源包
                    ResourcesComponent.Instance.LoadBundle(configBundleName);
                    
                    foreach (Type configType in configTypes) { // 遍历：ET 框架，事件系统，扫到的【客户端】所有Config 标签类型
						// 从资源包里，加载类型。。
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
 // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】