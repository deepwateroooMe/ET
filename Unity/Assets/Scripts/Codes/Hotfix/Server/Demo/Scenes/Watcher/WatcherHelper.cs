using System;
using System.Collections;
using System.Diagnostics;
namespace ET.Server {
    // 这个静态监控类：像是服务端【有个专用进程AppType.Watcher 工监进程】的监控帮助类：
    public static class WatcherHelper {
        public static StartMachineConfig GetThisMachineConfig() { // 获取本物理机配置
            string[] localIP = NetworkHelper.GetAddressIPs();
            StartMachineConfig startMachineConfig = null;
            foreach (StartMachineConfig config in StartMachineConfigCategory.Instance.GetAll().Values) { // 遍历了【服务端】的所有物理机
                if (!WatcherHelper.IsThisMachine(config.InnerIP, localIP)) 
                    continue;
                startMachineConfig = config; // 拿到本物理机的配置
                break;
            }
            if (startMachineConfig == null) 
                throw new Exception("not found this machine ip config!");
            return startMachineConfig;
        }
        public static bool IsThisMachine(string ip, string[] localIPs) {
            if (ip != "127.0.0.1" && ip != "0.0.0.0" && !((IList) localIPs).Contains(ip)) 
                return false;
            return true;
        }
// 是那台监视服务器，在监控或是重启被监视的宕机机器。。全局只有【监视看管、专职进程】添加这个组件 WatcherComponent. 像亲爱的表哥的活宝妹楼上的恶房东。。。
		// 就只有【监视看管、专职进程】：它通过 WatcherComponent 组件、负责，如下命令式启动某进程下的系列场景
        public static Process StartProcess(int processId, int createScenes = 0) { 
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(processId); // 拿到进程配置？进程所属的物理机的配置？
            // 从上面拿的是：宕机进程来看，下面的命令行，是以核为单位，重启某台物理机上N 多核中的某一个核的，命令行命令与参数。参数都可以框架里自己借用来填不会的空。。
            const string exe = "dotnet";
            string arguments = $"App.dll" + 
                $" --Process={startProcessConfig.Id}" +
                $" --AppType=Server" +  
                $" --StartConfig={Options.Instance.StartConfig}" +
                $" --Develop={Options.Instance.Develop}" +
                $" --CreateScenes={createScenes}" +
                $" --LogLevel={Options.Instance.LogLevel}" +
                $" --Console={Options.Instance.Console}";
            Log.Debug($"{exe} {arguments}");
            Process process = ProcessHelper.Run(exe, arguments);
            return process;
        }
    }
}