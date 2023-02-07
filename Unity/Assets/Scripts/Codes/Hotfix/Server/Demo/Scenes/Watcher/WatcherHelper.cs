using System;
using System.Collections;
using System.Diagnostics;

namespace ET.Server {
    public static class WatcherHelper {

        public static StartMachineConfig GetThisMachineConfig() {
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
        
        public static Process StartProcess(int processId, int createScenes = 0) {
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(processId);
// 下面的,就跟我昨天上午从emacs 中配置:  用VSCode打开当前emacs buffer文件并定位到某行某列是一样的,配置命令行工具,配置命令名称,参数等,用于执行服务器分区分服 某台特殊参数配置的 服务器的开启
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