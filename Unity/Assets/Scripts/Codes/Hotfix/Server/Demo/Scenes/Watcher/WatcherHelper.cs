using System;
using System.Collections;
using System.Diagnostics;
namespace ET.Server {
	// 每台物理机一个守护进程：负责看护该台物理机上所有进程的动作、宕机与重启等
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    public static class WatcherHelper {
        public static StartMachineConfig GetThisMachineConfig() { // 去拿：守候进程所在的、物理机的配置
            string[] localIP = NetworkHelper.GetAddressIPs();
            StartMachineConfig startMachineConfig = null;
            foreach (StartMachineConfig config in StartMachineConfigCategory.Instance.GetAll().Values) {
                if (!WatcherHelper.IsThisMachine(config.InnerIP, localIP)) {
                    continue;
                }
                startMachineConfig = config;
                break;
            }
            if (startMachineConfig == null) {
                throw new Exception("not found this machine ip config!");
            }
            return startMachineConfig;
        }
        public static bool IsThisMachine(string ip, string[] localIPs) {
            if (ip != "127.0.0.1" && ip != "0.0.0.0" && !((IList) localIPs).Contains(ip)) {
                return false;
            }
            return true;
        }
		// 根据本物理机的配置Options 和配置文件，使用命令行命令，来重启进程的命令、过程、逻辑
        public static Process StartProcess(int processId, int createScenes = 0) {
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(processId);
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