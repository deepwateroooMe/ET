using System;
using System.Collections.Generic;
using System.IO;
namespace ET.Server { // 【服务端】
// Invoke 标签：【服务端】、触发回调类 GetAllConfigBytes 的定义、实现逻辑。框架里基本上只有、这相关2 个类的 Invoke 用例
// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    [Invoke]  
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, byte[]>> {
        public override Dictionary<Type, byte[]> Handle(ConfigComponent.GetAllConfigBytes args) {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
			// 给几个：服务端关键配置的字符串
            List<string> startConfigs = new List<string>() {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
			// 【Config】：这里有个，双端任何一端启动时，根据Excel 配置文件、利用ExcelExporter 工具，写Options 启动各进程，并写配置文件的过程
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes) {
                string configFilePath;
                if (startConfigs.Contains(configType.Name)) 
                    configFilePath = $"../Config/Excel/s/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                else 
                    configFilePath = $"../Config/Excel/s/{configType.Name}.bytes";
                output[configType] = File.ReadAllBytes(configFilePath); // 从配置文件中读出来的结果，并返回
            }
            return output;
        }
    }
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigComponent.GetOneConfigBytes, byte[]> {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args) {
            byte[] configBytes = File.ReadAllBytes($"../Config/{args.ConfigName}.bytes");
			// 上面的 .bytes 文件，是哪里，什么时候生成的？是AppType.Tools 里 ExcelExporter 工具导出来，导在【服务端】的Config 路径下的多个 .bytes 文件
            return configBytes;
        }
    }
}
 // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
 // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】