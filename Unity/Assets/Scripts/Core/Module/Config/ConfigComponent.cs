using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ET {
    // Config组件会扫描所有的有【Config】标签的配置,加载进来
    public class ConfigComponent: Singleton<ConfigComponent> {
        public struct GetAllConfigBytes {  }
        public struct GetOneConfigBytes {
            public string ConfigName;// 只是用一个字符串来区分不同配置 
        }
        private readonly Dictionary<Type, ISingleton> allConfig = new Dictionary<Type, ISingleton>();
        public override void Dispose() {
            foreach (var kv in this.allConfig) {
                kv.Value.Destroy();
            }
        }
        public object LoadOneConfig(Type configType) {
            this.allConfig.TryGetValue(configType, out ISingleton oneConfig);// oneConfig：这里算是自定义变量的【申明与赋值】？
            if (oneConfig != null) {
                oneConfig.Destroy();
            } 
            // 跟进Invoke: 去看一下框架里事件系统，找到具体的激活回调逻辑定义类：ConfigLoader.cs, 去查看里面对 GetOneConfigBytes 类型的激活触发逻辑
            byte[] oneConfigBytes = EventSystem.Instance.Invoke<GetOneConfigBytes, byte[]>(new GetOneConfigBytes() {ConfigName = configType.FullName});
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
            ISingleton singleton = category as ISingleton;
            singleton.Register(); // 【单例类初始化】：如果已经初始化过，会抛异常；单例类只初始化一次
            this.allConfig[configType] = singleton; // 底层：管理类单例类，不同类型，各有一个。框架里就有上面看过的四大单例类
            return category;
        }
        public void Load() { // 【加载】：系统加载，程序域加载 
            this.allConfig.Clear(); // 清空
            // 【原理】：借助框架强大事件系统，扫描域里【Invoke|()】标签（2 种）；根据参数类型，调用触发激活逻辑，到服务端特定路径特定文件中去读取所有相关配置，并返回字典
            Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());
            foreach (Type type in configBytes.Keys) {
                byte[] oneConfigBytes = configBytes[type];
                this.LoadOneInThread(type, oneConfigBytes);
            }
        }
        public async ETTask LoadAsync() { // 哪里会调用这个方法？Entry.cs 服务端起来的时候，会调用此底层组件，加载各单例管理类。细看一下这里服务端启动初始化逻辑
            this.allConfig.Clear();
            Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());
            using ListComponent<Task> listTasks = ListComponent<Task>.Create();
            foreach (Type type in configBytes.Keys) {
                byte[] oneConfigBytes = configBytes[type];
// 四大单例管理类（Machine,Process,Scene,Zone）：每个单例类，开一个任务线路去完成？好像是这样的。
// 不明白为什么必须管理那四个，多不同场景可以位于同一进程，一台机器可以多核多进程？区区区。。。不明白
                Task task = Task.Run(() => LoadOneInThread(type, oneConfigBytes)); 
                listTasks.Add(task);
            }
            await Task.WhenAll(listTasks.ToArray());
        }
        
        private void LoadOneInThread(Type configType, byte[] oneConfigBytes) {
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
            lock (this) {
                ISingleton singleton = category as ISingleton;
                singleton.Register(); // 注册单例类：就是启动初始化一个单例类吧，框架里 Invoke 配置相关，有四大单例类
                this.allConfig[configType] = singleton;
            }
        }
    }
}
