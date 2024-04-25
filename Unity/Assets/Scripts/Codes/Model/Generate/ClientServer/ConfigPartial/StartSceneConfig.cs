using System.Collections.Generic; 
using System.ComponentModel;
using System.Net;
namespace ET { // 双端模式下、四大配制某配制的、部分类逻辑，回调等
    public partial class StartSceneConfigCategory {
        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>();
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ClientScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();
        public StartSceneConfig LocationConfig;
        public List<StartSceneConfig> Realms = new List<StartSceneConfig>();
        public List<StartSceneConfig> Routers = new List<StartSceneConfig>();
        public List<StartSceneConfig> Robots = new List<StartSceneConfig>();
        public StartSceneConfig BenchmarkServer;
        public List<StartSceneConfig> GetByProcess(int process) {
            return this.ProcessScenes[process];
        }
        public StartSceneConfig GetBySceneName(int zone, string name) {
            return this.ClientScenesByName[zone][name];
        }
		// 回调事件：启动结束后的、分门别类的、总管、管理逻辑
        public override void AfterEndInit() { // 查：它弄完后，又干了哪些自动回调事件
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values) { // 遍历，每个场景
				// 场景、所属的进程，管理 
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);
				// 各场景的分区、管理：【k,【k,v】】＝【所属区，【场景名、场景配置】】
                if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone)) {
                    this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ClientScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);
                switch (startSceneConfig.Type) {
                    case SceneType.Realm:
                        this.Realms.Add(startSceneConfig);
                        break;
                    case SceneType.Gate:
                        this.Gates.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Location:
                        this.LocationConfig = startSceneConfig;
                        break;
                    case SceneType.Robot:
                        this.Robots.Add(startSceneConfig);
                        break;
                    case SceneType.Router:
                        this.Routers.Add(startSceneConfig);
                        break;
                    case SceneType.BenchmarkServer:
                        this.BenchmarkServer = startSceneConfig;
                        break;
                }
            }
        }
    }
    public partial class StartSceneConfig: ISupportInitialize {
        public long InstanceId;
        public SceneType Type;
        public StartProcessConfig StartProcessConfig {
            get {
                return StartProcessConfigCategory.Instance.Get(this.Process);
            }
        }
        public StartZoneConfig StartZoneConfig {
            get {
                return StartZoneConfigCategory.Instance.Get(this.Zone);
            }
        }
        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint innerIPOutPort;
        public IPEndPoint InnerIPOutPort {
            get {
                if (innerIPOutPort == null) {
                    this.innerIPOutPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.InnerIP}:{this.OuterPort}");
                }
                return this.innerIPOutPort;
            }
        }
        private IPEndPoint outerIPPort;
        // 外网地址外网端口
        public IPEndPoint OuterIPPort {
            get {
                if (this.outerIPPort == null) {
                    this.outerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.OuterPort}");
                }
                return this.outerIPPort;
            }
        }
		// 对于【场景】配制来说，完后，就自动生成的实例号
        public override void AfterEndInit() {
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.InstanceId = instanceIdStruct.ToLong();
        }
    }
}