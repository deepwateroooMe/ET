using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
namespace ET {

    public partial class StartSceneConfigCategory { // 【双端】
        // 数据结构：MultiMap, 它的值，实际是K 类型的链表，不止一个，是链表
        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>(); // 【进程场景】：现在还没有弄明白，进程场景，从源码上区分的特殊性。活宝妹把它想成一台物理机一个核上的主线程
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ClientScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();
        // 【位置服】：可以只有一个，可以有分身与备份。这里所拿到的，可以是全局唯一的位置服，也可以是如同【网关服】【数据库】一样分配在当前小区下的【位置服】
        // 最主要的，这里拿到的，或全局唯一【位置服】；或至少【随机分配】、或按小区分配给，当前物理机当前核当前进程当前场景的，它可以使用的【位置服】索引
        public StartSceneConfig LocationConfig; 
        public StartSceneConfig Realm; // 可是这里感觉，好像是ET7 重构成这样的，【改天，去对比一下】
        public StartSceneConfig Match; // 添加管理：不再使用一条链表，全局一个【匹配服】
        public List<StartSceneConfig> Routers = new List<StartSceneConfig>();
        public List<StartSceneConfig> Robots = new List<StartSceneConfig>();
        public StartSceneConfig BenchmarkServer;
        public List<StartSceneConfig> GetByProcess(int process) {
            return this.ProcessScenes[process];
        }
        public StartSceneConfig GetBySceneName(int zone, string name) {
            return this.ClientScenesByName[zone][name];
        }
        // AfterEndInit() 接口定义的地方：ProtoObject。那么，根据Json.txt 配置和启动服务端，算怎么回事呢？；这里更像是，各小服【自底向上】上报管理端，本小服配置信息？
        // AfterEndInit() 是跨进程消息，ProtoObject 类里所定义的【反序列化】结束后，的回调。那么，更像自己所理解的【自底向上】上报的过程 
        public override void AfterEndInit() { // 【初始化】结束时的回调：添加这些事项
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values) {
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig); // 进程管理 
                if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone)) {  // 场景管理 
                    this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ClientScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);
                switch (startSceneConfig.Type) { // 其它添加管理事项
                    case SceneType.Realm:
                        this.Realm = startSceneConfig;
                        break;
                    case SceneType.Match: // 对【匹配服】的管理
                        this.Match = startSceneConfig;
                        break;
                    case SceneType.Gate: // 网关：小区区号，与配置 
                        this.Gates.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Location: // <<<<<<<<<<<<<<<<<<<< 【原本的】：就是假定了一个核进程下，仅存在一个【本小区位置服】
                        this.LocationConfig = startSceneConfig;
                        break;
                    case SceneType.Robot:
                        this.Robots.Add(startSceneConfig);
                        break;
                    case SceneType.Router: // 【路由器】场景：是自己现在看的重点
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
            get { // 从下面这行来看，每台小服、专职服都有他们各自的初始化场景？这里问得什么牛头不对马尾的？！！明明是，Process 进程的配置
                return StartProcessConfigCategory.Instance.Get(this.Process); // 通过进程号，去拿各小服专职服的【进程初始化配置】
            }
        }
        public StartZoneConfig StartZoneConfig {
            get {
                return StartZoneConfigCategory.Instance.Get(this.Zone);
            }
        }
        // 内网地址外网端口，通过防火墙映射端口过来：【通过防火墙映射端口过来】没看懂。。
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