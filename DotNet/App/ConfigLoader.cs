using System;
using System.Collections.Generic;
using System.IO;
namespace ET.Server {
    [Invoke] // 激活系: 这个激活系是同属ET 强大的事件系统的一个标签和回调逻辑，处理两种类型： GetAllConfigBytes 和 GetOneConfigBytes
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, byte[]>> {
        public override Dictionary<Type, byte[]> Handle(ConfigComponent.GetAllConfigBytes args) {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            List<string> startConfigs = new List<string>() {
                "StartMachineConfigCategory",  // 涉及底层配置的几个单例类，为什么这四个单例类类型重要： Machine, Process 进程、Scene 场景， Zone 区
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
// 类型：这里，扫的是所有【Invoke】标签（好像不对），还是说如【Invoke(TimerInvokeType.ActorMessegaeSenderChecker)】之类的Invoke 标签的类型属性？去看一下方法定义
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute)); // 【Config】标签
            foreach (Type configType in configTypes) {
                string configFilePath;
                if (startConfigs.Contains(configType.Name)) { // 就是，配置文件的地址可能不一样
                    configFilePath = $"../Config/Excel/s/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                }
                else {
                    configFilePath = $"../Config/Excel/s/{configType.Name}.bytes";
                }
                output[configType] = File.ReadAllBytes(configFilePath);
            }
            return output;
        }
    }
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigComponent.GetOneConfigBytes, byte[]> {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args) {
            // 【Invoke 回调逻辑】：从框架特定位置，读取特定属性条款的配置，返回字节数组
            byte[] configBytes = File.ReadAllBytes($"../Config/{args.ConfigName}.bytes");
            return configBytes;
        }
    }
}