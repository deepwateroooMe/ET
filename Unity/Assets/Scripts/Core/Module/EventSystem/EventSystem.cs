using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET {

    public class EventSystem: Singleton<EventSystem>, ISingletonUpdate, ISingletonLateUpdate {

        private class OneTypeSystems {
            public readonly UnOrderMultiMap<Type, object> Map = new(); // Dictionary [type, List<object>]
            // 这里不用hash，数量比较少，直接for循环速度更快 [因为长度只有 4 ]
            public readonly bool[] QueueFlag = new bool[(int)InstanceQueueIndex.Max];
        }
        // 双端 类型管理系统:　服务端客户端
        private class TypeSystems {
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new(); // Dic [Type, Map/QueueFlag ]
            public OneTypeSystems GetOrCreateOneTypeSystems(Type type) {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null) {
                    return systems;
                }
                systems = new OneTypeSystems(); // 不存在的时候,实例化一个出来 
                this.typeSystemsMap.Add(type, systems);
                return systems;
            }
            public OneTypeSystems GetOneTypeSystems(Type type) {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }
            public List<object> GetSystems(Type type, Type systemType) {
                OneTypeSystems oneTypeSystems = null;
                if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems)) {
                    return null;
                }
                if (!oneTypeSystems.Map.TryGetValue(systemType, out List<object> systems)) {
                    return null;
                }
                return systems;
            }
        }

        private class EventInfo {

            public IEvent IEvent { get; } // Type
            public SceneType SceneType {get; } // 情境 上下文标记 

            public EventInfo(IEvent iEvent, SceneType sceneType) {
                this.IEvent = iEvent;
                this.SceneType = sceneType;
            }
        }
        
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new(); // 无序　Dic<type, HashSet<Type>>　
        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();
        
        private Dictionary<Type, Dictionary<int, object>> allInvokes = new(); // 所有的回调　？
        private TypeSystems typeSystems = new();　// 
        private readonly Queue<long>[] queues = new Queue<long>[(int)InstanceQueueIndex.Max]; // unity游戏事件回调类型,只有最多4种:　Update() LateUpdate() Load() Max ?
        public EventSystem() {
            for (int i = 0; i < this.queues.Length; i++) {
                this.queues[i] = new Queue<long>(); // 初始化，无数据填充
            }
        }
// Add: 这个比较复杂一点儿: 相当于是全局初始化时的配置,所有的类型事件扫描一遍,该清空清空,该创建创建,该实例化实例化.有初始化时的系统化管理在里面
// 基本上,对所有几大类进行管理的数据结构:清空，或是新分配内存空间.应该是全局(一次程序集加载)只初始化调用一次        
        public void Add(Dictionary<string, Type> addTypes) {
            this.allTypes.Clear(); // <<<<<<<<<<<<<<<<<<<< 
            this.types.Clear();    // <<<<<<<<<<<<<<<<<<<< 
            
            foreach ((string fullName, Type type) in addTypes) {
                this.allTypes[fullName] = type;
                
                if (type.IsAbstract) {
                    continue;　// 抽象类: 跳过
                 }
// BaseAttribute: 双端应用中,所有感兴趣的共同基类,全局綂一管理                
                // 记录所有的有 BaseAttribute 标记的的类型: [这是一个全局自定义标记，用来对类型事件回调等进行管理]
                // 双端 有狠多 BaseAttribute 的实体继承子类:这里是对 BaseAttribute 这一自定义属性 所有继承实体子类(class)的 注册管理
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);　// 提取到所有标记过[BaseAttribute]自定义标签的类型
                foreach (object o in objects) {
                    this.types.Add(o.GetType(), type);
                }
            }
// 下面是双端应用中的三大相对基类类型: ObjectSystemAttribute, EventAttribute, InvokeAttribute           
// ObjectSystemAttribute            
            this.typeSystems = new TypeSystems(); // <<<<<<<<<<<<<<<<<<<<
            foreach (Type type in this.GetTypes(typeof (ObjectSystemAttribute))) {
                object obj = Activator.CreateInstance(type); // 创建类(class) 的一个实例
                if (obj is ISystemType iSystemType) {
                    OneTypeSystems oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj); // 双端的自定义类型(相当于是分组)标记(组的编号)
                    InstanceQueueIndex index = iSystemType.GetInstanceQueueIndex();
                    if (index > InstanceQueueIndex.None && index < InstanceQueueIndex.Max) {
                        oneTypeSystems.QueueFlag[(int)index] = true;
                    }
                }
            }
// EventAttribute: 触发事件的相关类型class
            this.allEvents.Clear(); // <<<<<<<<<<<<<<<<<<<< 
            foreach (Type type in types[typeof (EventAttribute)]) {
                IEvent obj = Activator.CreateInstance(type) as IEvent; // <<<<<<<<<< 先创建IEvent实例,方便系统化管理 
                if (obj == null) {
                    throw new Exception($"type not is AEvent: {type.Name}");
                }
                object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
                foreach (object attr in attrs) {
                    EventAttribute eventAttribute = attr as EventAttribute;
                    Type eventType = obj.Type;
                    EventInfo eventInfo = new(obj, eventAttribute.SceneType); // <<<<<<<<<< 再创建 EventInfo 实例,方便注册管理
                    if (!this.allEvents.ContainsKey(eventType)) {
                        this.allEvents.Add(eventType, new List<EventInfo>());
                    }
                    this.allEvents[eventType].Add(eventInfo); // 表示: 对当前eventType类型,感兴趣(有回调的)所有相关事件类型(eventInfo)[表示触发事件的参数与回调类型等?]
                }
            }
// InvokeAttribute: 会涉及事件回调处理 callbacks           
            this.allInvokes = new Dictionary<Type, Dictionary<int, object>>(); // 这里是创建新的,是因为源码中管理得好的回调,会自动取消注册.先前的空字典会被系统自动回收,不会资源泄露
            foreach (Type type in types[typeof (InvokeAttribute)]) {
                object obj = Activator.CreateInstance(type); // <<<<<<<<<< 
                IInvoke iInvoke = obj as IInvoke;
                if (iInvoke == null) {
                    throw new Exception($"type not is callback: {type.Name}");
                }
                object[] attrs = type.GetCustomAttributes(typeof(InvokeAttribute), false);
                foreach (object attr in attrs) {
                    if (!this.allInvokes.TryGetValue(iInvoke.Type, out var dict)) {
                        dict = new Dictionary<int, object>(); // <<<<<<<<<< 
                        this.allInvokes.Add(iInvoke.Type, dict);
                    }
                    InvokeAttribute invokeAttribute = attr as InvokeAttribute;
                    try {
                        dict.Add(invokeAttribute.Type, obj);
                    }
                    catch (Exception e) {
                        throw new Exception($"action type duplicate: {iInvoke.Type.Name} {invokeAttribute.Type}", e);
                    }
                    
                }
            }
        }
        public HashSet<Type> GetTypes(Type systemAttributeType) {
            if (!this.types.ContainsKey(systemAttributeType)) {
                return new HashSet<Type>();
            }
            return this.types[systemAttributeType];
        }
        public Dictionary<string, Type> GetTypes() {
            return allTypes;
        }
        public Type GetType(string typeName) {
            return this.allTypes[typeName];
        }
        public void RegisterSystem(Entity component) {
            Type type = component.GetType();
            OneTypeSystems oneTypeSystems = this.typeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null) {
                return;
            }
            for (int i = 0; i < oneTypeSystems.QueueFlag.Length; ++i) {
                if (!oneTypeSystems.QueueFlag[i]) {
                    continue;
                }
                this.queues[i].Enqueue(component.InstanceId);
            }
        }
        public void Deserialize(Entity component) {
            List<object> iDeserializeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null) {
                return;
            }
            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems) {
                if (deserializeSystem == null) {
                    continue;
                }
                try {
                    deserializeSystem.Run(component);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        
        // GetComponentSystem
        public void GetComponent(Entity entity, Entity component) {
            List<object> iGetSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSystem));
            if (iGetSystem == null) {
                return;
            }
            foreach (IGetComponentSystem getSystem in iGetSystem) {
                if (getSystem == null) {
                    continue;
                }
                try {
                    getSystem.Run(entity, component);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        
        // AddComponentSystem
        public void AddComponent(Entity entity, Entity component) {
            List<object> iAddSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IAddComponentSystem));
            if (iAddSystem == null) {
                return;
            }
            foreach (IAddComponentSystem addComponentSystem in iAddSystem) {
                if (addComponentSystem == null) {
                    continue;
                }
                try {
                    addComponentSystem.Run(entity, component);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Awake(Entity component) {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null) {
                return;
            }
            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems) {
                if (aAwakeSystem == null) {
                    continue;
                }
                try {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Awake<P1>(Entity component, P1 p1) {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null) {
                return;
            }
            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems) {
                if (aAwakeSystem == null) {
                    continue;
                }
                try {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2) {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null) {
                return;
            }
            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems) {
                if (aAwakeSystem == null) {
                    continue;
                }
                try {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3) {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null) {
                return;
            }
            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems) {
                if (aAwakeSystem == null) {
                    continue;
                }
                try {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4) {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null) {
                return;
            }
            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems) {
                if (aAwakeSystem == null) {
                    continue;
                }
                try {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
// 我记得这里，自己好像是看过一遍的：        
        public void Load() {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.Load];
            int count = queue.Count;
            while (count-- > 0) {  // 它会把这个队列遍历一遍，清除掉过期不合法的，只保留当前有效的，
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null) {
                    continue;
                }
                if (component.IsDisposed) {
                    continue;
                }
                List<object> iLoadSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILoadSystem));
                if (iLoadSystems == null) {
                    continue;
                }
                queue.Enqueue(instanceId);
                foreach (ILoadSystem iLoadSystem in iLoadSystems) { // 会把队列中当前的合法元素，作必要的加载
                    try {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e) {
                        Log.Error(e);
                    }
                }
            }
        }
        public void Destroy(Entity component) {
            List<object> iDestroySystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null) {
                return;
            }
            foreach (IDestroySystem iDestroySystem in iDestroySystems) {
                if (iDestroySystem == null) {
                    continue;
                }
                try {
                    iDestroySystem.Run(component);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public void Update() {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.Update];
            int count = queue.Count;
            while (count-- > 0) {
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null) {
                    continue;
                }
                if (component.IsDisposed) {
                    continue;
                }
                List<object> iUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                if (iUpdateSystems == null) {
                    continue;
                }
                queue.Enqueue(instanceId);
                foreach (IUpdateSystem iUpdateSystem in iUpdateSystems) {
                    try {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e) {
                        Log.Error(e);
                    }
                }
            }
        }
        public void LateUpdate() {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.LateUpdate];
            int count = queue.Count;
            while (count-- > 0) {
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null) {
                    continue;
                }
                if (component.IsDisposed) {
                    continue;
                }
                List<object> iLateUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null) {
                    continue;
                }
                queue.Enqueue(instanceId);
                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems) {
                    try {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e) {
                        Log.Error(e);
                    }
                }
            }
        }
        public async ETTask PublishAsync<T>(Scene scene, T a) where T : struct {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents)) {
                return;
            }
            using ListComponent<ETTask> list = ListComponent<ETTask>.Create();
            
            foreach (EventInfo eventInfo in iEvents) {
                if (scene.SceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None) {
                    continue;
                }
                    
                if (!(eventInfo.IEvent is AEvent<T> aEvent)) {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }
                list.Add(aEvent.Handle(scene, a));
            }
            try {
                await ETTaskHelper.WaitAll(list);
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
        public void Publish<T>(Scene scene, T a) where T : struct {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents)) {
                return;
            }
            SceneType sceneType = scene.SceneType;
            foreach (EventInfo eventInfo in iEvents) {
                if (sceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None) {
                    continue;
                }
                
                if (!(eventInfo.IEvent is AEvent<T> aEvent)) {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }
                
                aEvent.Handle(scene, a).Coroutine();
            }
        }
        
        // Invoke跟Publish的区别(特别注意)
        // Invoke类似函数，必须有被调用方，否则异常，调用者跟被调用者属于同一模块，比如MoveComponent中的Timer计时器，调用跟被调用的代码均属于移动模块
        // 既然Invoke跟函数一样，那么为什么不使用函数呢? 因为有时候不方便直接调用，比如Config加载，在客户端跟服务端加载方式不一样。比如TimerComponent需要根据Id分发
        // 注意，不要把Invoke当函数使用，这样会造成代码可读性降低，能用函数不要用Invoke
        // publish是事件，抛出去可以没人订阅，调用者跟被调用者属于两个模块，比如任务系统需要知道道具使用的信息，则订阅道具使用事件
        public void Invoke<A>(int type, A args) where A: struct {
            if (!this.allInvokes.TryGetValue(typeof(A), out var invokeHandlers)) {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler)) {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }
            var aInvokeHandler = invokeHandler as AInvokeHandler<A>;
            if (aInvokeHandler == null) {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(A).Name} {type}");
            }
            
            aInvokeHandler.Handle(args);
        }
        
        public T Invoke<A, T>(int type, A args) where A: struct {
            if (!this.allInvokes.TryGetValue(typeof(A), out var invokeHandlers)) {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler)) {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }
            var aInvokeHandler = invokeHandler as AInvokeHandler<A, T>;
            if (aInvokeHandler == null) {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(T).Name} {type}");
            }
            
            return aInvokeHandler.Handle(args);
        }
        
        public void Invoke<A>(A args) where A: struct
        {
            Invoke(0, args);
        }
        
        public T Invoke<A, T>(A args) where A: struct
        {
            return Invoke<A, T>(0, args);
        }
    }
}
