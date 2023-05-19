using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
namespace ET {
    // 配置文件处理，或是服务器启动相关类，以前都没仔细读过
    public partial class StartSceneConfigCategory { // 【双端】

        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>();
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ClientScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();
        public StartSceneConfig LocationConfig;
        public List<StartSceneConfig> Realms = new List<StartSceneConfig>();
        public List<StartSceneConfig> Matchs = new List<StartSceneConfig>(); // <<<<<<<<<<<<<<<<<<<< 添加管理
        public List<StartSceneConfig> Routers = new List<StartSceneConfig>();
        public List<StartSceneConfig> Robots = new List<StartSceneConfig>();
        public StartSceneConfig BenchmarkServer;

        public List<StartSceneConfig> GetByProcess(int process) {
            return this.ProcessScenes[process];
        }
        public StartSceneConfig GetBySceneName(int zone, string name) {
            return this.ClientScenesByName[zone][name];
        }
        public override void AfterEndInit() {
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values) {
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);
                
                if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone)) {
                    this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ClientScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);
                
                switch (startSceneConfig.Type) {
                        case SceneType.Realm:
                            this.Realms.Add(startSceneConfig);
                            break;
                        case SceneType.Match: // 对【匹配服】的管理, 参照登录服来的
                            this.Matchs.Add(startSceneConfig);
                            break;
                        case SceneType.Gate: // 网关：小区区号，与配置 
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
                        case SceneType.BenchmarkServer: //【特殊】：因为它只有一个
                            this.BenchmarkServer = startSceneConfig;
                            break;
                }
            }
        }
    }
    public partial class StartSceneConfig: ISupportInitialize {
        public long InstanceId;
        public SceneType Type; // 场景类型

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
        // 外网地址外网端口
        private IPEndPoint outerIPPort;
        public IPEndPoint OuterIPPort {
            get {
                if (this.outerIPPort == null) {
                    this.outerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.OuterPort}");
                }
                return this.outerIPPort;
            }
        }
        public override void AfterEndInit() {
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.InstanceId = instanceIdStruct.ToLong();
        }
    }
}




