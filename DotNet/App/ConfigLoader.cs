using System;
using System.Collections.Generic;
using System.IO;
namespace ET.Server {
    [Invoke] // 激活系: 这个激活系是同属ET 强大的事件系统的一个标签和回调逻辑，处理两种类型： GetAllConfigBytes 和 GetOneConfigBytes
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, byte[]>> {
// 【服务端】命令行的启动、起始，的大致过程：
    // 命令行，各种参数，下发命令，启动服务端【这里细节去找】：是以物理机、进程、场景、Zone 为启动单位，可以在各个不同层面启动的吗？还是只能物理机？
    // 框架里封装过的命令行解析：解析入Options 单例类，以及如下【四大文件】主要管理，以及其它小兵小将文件，如 UnitConfigCategory.bytes 里
    // 这个文件提供两种查询方式：拿到全局所有配置，和，可以来拿来读某种特例配置。拿的方式是：这里去到了初始配置时写的几个文件里去读，读所有，和读 specific
        public override Dictionary<Type, byte[]> Handle(ConfigComponent.GetAllConfigBytes args) {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>(); // 准备一个小本字典：准备记笔记
            // 它分了这几类来管理，然后就把【服务端】起来时的各种配置，写入了这几类文件。要用的时候，从保存过的这些配置文件里去读。。。？
            List<string> startConfigs = new List<string>() { // 几个类型的区分：去看对这几种类型的管理，什么内容添加进了它们的管理里？
                "StartMachineConfigCategory",  // 涉及底层配置的几个单例类，为什么这四个单例类类型重要： Machine, Process 进程、Scene 场景， Zone 区
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
// 类型：这里，扫的是所有【Config】标签
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute)); // 【Config】标签：返回程序域里所有的【Config】标签类型
            foreach (Type configType in configTypes) {
                string configFilePath;
// 【路径中的参数】：Options.Instance, 不是说是从命令行参数解析进单例类里的吗？那么【服务端】最初的激活启动是来自于命令行的命令。【服务端命令行启动】，功能，要在框架里找一找
                if (startConfigs.Contains(configType.Name)) { // 【单例管理类型】：有特异性的配置路径，读的都是【仅服务端】文件夹中的配置，不适用于【双端】模式。读命令行中的一个参数
                    configFilePath = $"../Config/Excel/s/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                } else { // 其它：人海里的路人甲，读下配置就扔掉。 框架里保存过的配置包括： UnitConfigCategory.bytes 和另外一个
                    configFilePath = $"../Config/Excel/s/{configType.Name}.bytes"; 
                }
                output[configType] = File.ReadAllBytes(configFilePath); // 把读到的配置，一条一条记入小本
            }
            return output; // 返回所有配置 
        }
    }
    [Invoke] // 这个标签的激活：现在框架里，好像还不存在，任何一个已经写过的 {args.ConfigName}.bytes 的例子，还没用到？
    public class GetOneConfigBytes: AInvokeHandler<ConfigComponent.GetOneConfigBytes, byte[]> {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args) {
            // 【Invoke 回调逻辑】：从框架特定位置，读取特定属性条款的配置，返回字节数组
            byte[] configBytes = File.ReadAllBytes($"../Config/{args.ConfigName}.bytes");
            return configBytes;
        }
    }
}