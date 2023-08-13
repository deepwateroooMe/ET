using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;
namespace ET { // 感觉这个文件的标签稍微少一点儿: 【IMerge 接口Merge() 方法】：为什么就找不到使用的地方？在ExcelExporter.cs 等工具类的地方，有用到，去看下能否看懂。。。
    // 暂时把这个知道点跳过，改天再回来捡。。。
    [ProtoContract]
    [Config]
    public partial class StartMachineConfigCategory : ConfigSingleton<StartMachineConfigCategory>, IMerge { //
        // 重点看：下面两个变量的管理：如何更新的
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, StartMachineConfig> dict = new Dictionary<int, StartMachineConfig>();
        [BsonElement]
        [ProtoMember(1)]
        private List<StartMachineConfig> list = new List<StartMachineConfig>();
// 重点：找这个公用方法，调用的地方。框架找不到，活宝妹怀疑它是 protobuf 的库里。暂时放一下, 改天再回来看、和找这个细节
        // 现在，开始觉得：这个方法，是提供给【服务端】根据Json.txt 的各种配置文件，来合并同一类型的服务器配置的。比如StartZoneConfig, 无数个小区，小区间的合同会调用到IMerge 接口里的Merge 方法。慢慢会理解得更为透彻的
        // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public void Merge(object o) { // IMerge 接口类申明的这个方法: 或者我找到任何可能调用这里的方法
            StartMachineConfigCategory s = o as StartMachineConfigCategory;
            this.list.AddRange(s.list); // 更新链表：加入的可以是好几个元素的小链表。。。
        }
        [ProtoAfterDeserialization]        
        public void ProtoEndInit() {
            foreach (StartMachineConfig config in list) { // 每台机器的：初始化后回调，是这里调用的
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            this.AfterEndInit(); // 再次：本台物理机的层面，再调用一次，本台物理机，初始化结束时，可以做些什么的回调。体现出框架不同层面的回调层次
        }
        public StartMachineConfig Get(int id) {
            this.dict.TryGetValue(id, out StartMachineConfig item);
            if (item == null) 
                throw new Exception($"配置找不到，配置表名: {nameof (StartMachineConfig)}，配置id: {id}");
            return item;
        }
        public bool Contain(int id) {
            return this.dict.ContainsKey(id);
        }
        public Dictionary<int, StartMachineConfig> GetAll() {
            return this.dict;
        }
        public StartMachineConfig GetOne() { // 随便抓的一样：当前是哪个，就是哪个？或者说，当前空闲的？
            if (this.dict == null || this.dict.Count <= 0) 
                return null;
            return this.dict.Values.GetEnumerator().Current; // 指针，当前。。。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        }
    }
    [ProtoContract]
    public partial class StartMachineConfig: ProtoObject, IConfig {  
        // 是台物理机：就会有IP 地址，方便通信。这些，是自己迷迷糊糊的地方
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string InnerIP { get; set; } // 内网地址：大概是说，内网里机器之间可以通信
        [ProtoMember(3)]
        public string OuterIP { get; set; } // 外网地址：对外，只能看见外网地址
        [ProtoMember(4)]
        public string WatcherPort { get; set; }
    }
}
