using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
namespace ET {
    public partial class StartSceneConfigCategory {
        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>(); // 【服务端】场景管理
        // 【客户端】场景管理
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ClientScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();
        public StartSceneConfig LocationConfig;
        // 【一堆链表】：就是把同一类型的区服系统，再放回一个链表里，方便某小区用户，或是网关服要拿某种服地址的时候，可以相对快速地专服链表里去取
        public List<StartSceneConfig> Realms = new List<StartSceneConfig>();
        public List<StartSceneConfig> Matchs = new List<StartSceneConfig>(); // 我记得我有添加这个，可能是加在双端里 ?
        public List<StartSceneConfig> Routers = new List<StartSceneConfig>();
        public List<StartSceneConfig> Robots = new List<StartSceneConfig>();
        public StartSceneConfig BenchmarkServer;

        public List<StartSceneConfig> GetByProcess(int process) {
            return this.ProcessScenes[process];
        }
        public StartSceneConfig GetBySceneName(int zone, string name) {
            return this.ClientScenesByName[zone][name];
        }

        public override void AfterEndInit() { // 初始化结束时：对各场景各种服，进行分门别类，专职链表管理，方便查找 
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values) { // 遍历：初始化过的所有场景，各种小区服
                // ProcessScenes: 多字典，是什么意思呢？当ET7 里把各专职服重构为场景，这里是说，同一个进程里可能会存大多个不同的场景吗？
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);
                
                if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone)) {
                    this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ClientScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig); // 字典套字典：分区管理，区里的内容是嵌套的字典，名为键 
                
                switch (startSceneConfig.Type) {
                    case SceneType.Realm:
                        this.Realms.Add(startSceneConfig);
                        break;
                    case SceneType.Match: // 不知道这里加得对不对，再检查一下
                        this.Matchs.Add(startSceneConfig);
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
        public override void AfterEndInit() {
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.InstanceId = instanceIdStruct.ToLong();
        }
    }
}