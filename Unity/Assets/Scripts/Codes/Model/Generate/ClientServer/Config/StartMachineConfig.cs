using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;
namespace ET {
    // 这个类的标签太多了，并且大多早Protobuf 库里的标签，不看这个文件，看个标签少的文件！！【爱表哥，爱生活！！！】
    [ProtoContract]
    [Config]
    public partial class StartMachineConfigCategory : ConfigSingleton<StartMachineConfigCategory>, IMerge { // 实现了这个合并接口
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, StartMachineConfig> dict = new Dictionary<int, StartMachineConfig>();
        [BsonElement]
        [ProtoMember(1)]
        private List<StartMachineConfig> list = new List<StartMachineConfig>();
        public void Merge(object o) { // 实现接口里申明的方法: 可以去找一下，哪里调用了这个方法？
            StartMachineConfigCategory s = o as StartMachineConfigCategory;
            this.list.AddRange(s.list); // 这里就可以是，进程间可传递的消息，的自动合并
        }
        [ProtoAfterDeserialization] // Protobuf 里：定义的标签，使用它的库里的标签系
        public void ProtoEndInit() {
            foreach (StartMachineConfig config in list) {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            this.AfterEndInit();
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
        public StartMachineConfig GetOne() {
            if (this.dict == null || this.dict.Count <= 0) 
                return null;
            return this.dict.Values.GetEnumerator().Current;
        }
    }
    [ProtoContract]
    public partial class StartMachineConfig: ProtoObject, IConfig {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string InnerIP { get; set; }
        [ProtoMember(3)]
        public string OuterIP { get; set; }
        [ProtoMember(4)]
        public string WatcherPort { get; set; }
    }
}