using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ET {
    // Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
	// 感觉，亲爱的表哥的活宝妹，以前还没能把这个【双端、启动过程的、边边角角的、细节弄清楚】今天下午就看这个，两个半小时，看能够看懂多少
	
    public class ConfigComponent: Singleton<ConfigComponent> {
		// ET 双端：结构体的定义。虽然空，但是可以被双端【服务端、客户端】不报空地、使用相同的定义、来执行双端各自的逻辑。双端都有GetAllConfigBytes类定义？
        public struct GetAllConfigBytes {
		}
        public struct GetOneConfigBytes {
            public string ConfigName;
        }
        private readonly Dictionary<Type, ISingleton> allConfig = new Dictionary<Type, ISingleton>(); // 单例总管、的字典：任何被管理的类型，值也都是单例
        public override void Dispose() {
            foreach (var kv in this.allConfig) {
                kv.Value.Destroy();
            }
        }
		// 细节：这里、哪里？有个【Invoke】标签的触发机制。以前看过，忘记了再快速看一遍。好像不是这个方法里的。【服务端】启动时根据 Excel 里四大类型配置文件时的
		// 下面：LoadAsync() 协程的调用，会Invoke 双端任何一端的、自动扫描。
        public object LoadOneConfig(Type configType) {
            this.allConfig.TryGetValue(configType, out ISingleton oneConfig);
            if (oneConfig != null) {
                oneConfig.Destroy();
            }
            byte[] oneConfigBytes = EventSystem.Instance.Invoke<GetOneConfigBytes, byte[]>(new GetOneConfigBytes() {ConfigName = configType.FullName});
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
            ISingleton singleton = category as ISingleton;
            singleton.Register();
            this.allConfig[configType] = singleton;
            return category;
        }
        public void Load() {
            this.allConfig.Clear();
            Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());
            foreach (Type type in configBytes.Keys) {
                byte[] oneConfigBytes = configBytes[type];
                this.LoadOneInThread(type, oneConfigBytes); // <<<<<<<<<<<<<<<<<<<< 
            }
        }
		// 【双端、任何一端、配置扫描加载】：亲爱的表哥的活宝妹，以前看得好想当然，现在才感觉，好多细节不懂
		// 【ET 框架事件系统】：说是ET 心脏，感觉看懂了，可是事件机制里 Invoke 后，双端不同物理机、进程、各自回调，又跨进程返回配制？的过程
		// 这次，上面的这些细节，得看懂了
		// 先，去找：事件系统 Invoke 后，双端，哪些逻辑、逻辑主要步骤、原理
        public async ETTask LoadAsync() {
            this.allConfig.Clear();
			// 下面的Invoke() 调用：有个【Invoke】框架程序域、标签的封装、与自动调用，去看一下细节 
            Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());
			// 上面说：如果你是【服务端】，就去扫你的四大配置，启动那些不同的物理机、进程、场景、Zone之类的；如果你是【客户端】，也去执行你的客户端逻辑。。
			// 【客户端】的定义逻辑：还要再细看一下，还不太懂
			
            using ListComponent<Task> listTasks = ListComponent<Task>.Create();
            foreach (Type type in configBytes.Keys) {
                byte[] oneConfigBytes = configBytes[type]; // 类型： byte[] 所以下面要 Deserialize()
                Task task = Task.Run(() => LoadOneInThread(type, oneConfigBytes)); // 应该是开个异步任务来完成的。【单线程多进程】架构，开个线程，本质上是，开个进程？【TODO】：
                listTasks.Add(task);
            }
            await Task.WhenAll(listTasks.ToArray());
        }
        private void LoadOneInThread(Type configType, byte[] oneConfigBytes) {
            object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length); 
            lock (this) { // 继续上面的【单线程多进程】ET 架构，多进程下【服务端单例 ConfigComponent】，需要锁保障多进程安全。。
                ISingleton singleton = category as ISingleton;
                singleton.Register();
                this.allConfig[configType] = singleton;
            }
        }
    }
}
 // 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！