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
// 细节：这两行逻辑，实际已经实现了？：【服务端不关服热更新】。这里说，以前加载过的，就见鬼去吧。。重新加载新的。就可以实现重新加载热更新过的资源包，和重新更新配置？【任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
            if (oneConfig != null) 
                oneConfig.Destroy();
            // 跟进Invoke: 去看一下框架里事件系统，找到具体的激活回调逻辑定义类：ConfigLoader.cs, 去查看里面对 GetOneConfigBytes 类型的激活触发逻辑
            byte[] oneConfigBytes = EventSystem.Instance.Invoke<GetOneConfigBytes, byte[]>(new GetOneConfigBytes() {ConfigName = configType.FullName});
            // 下面的Deserialize: 不知道这些配置管理是 dotnet 服务端管理总后台？为什么需要反序列化？知道 byte 转对象有个步骤，可是反序列化？？？
            // 所以要去想，【服务端】的启动过程，它们的配置，是否由每个核、每个进程上的各个小服，如【动态路由系统】般，小伙伴云游般，跨进程？各自上报，到管理总后台的？如此，【序列化】【反序列】才更好理解。要去框架里去找。。。
            // Deserialize【序列化】与【反序列化】：【服务端】各小服的启动、配置【自底向上】自动上报、集合的过程？？？【服务端】根据必要的【热更新资源包】【自顶向下】不关服、动态更新配置的过程？【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length); // <<<<<<<<<<<<<<<<<<<< 
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
                this.LoadOneInThread(type, oneConfigBytes); // 每种配置：开一个线程去处理
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
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length); // 先反序列化
            lock (this) {
                ISingleton singleton = category as ISingleton;
                singleton.Register(); // 注册单例类：就是启动初始化一个单例类吧，框架里 Invoke 配置相关，有四大单例类
                this.allConfig[configType] = singleton;
            }
        }
    }
}
