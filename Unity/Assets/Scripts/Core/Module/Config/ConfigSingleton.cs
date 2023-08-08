using System;
namespace ET {
    public abstract class ConfigSingleton<T>: ProtoObject, ISingleton where T: ConfigSingleton<T>, new() {
        [StaticField]
        private static T instance;
        public static T Instance {
            get {
// 下面这里是：第一次配置的时候，去读或激活。它不能动态【不关服配置物理机】吗？好像是这样的。
                // 就是，【服务端】不关服，置换小服类型，应该可能是不太可能的【亲爱的表哥的活宝妹，奇特脑袋，纯理论突发奇想的。。】
                return instance ??= ConfigComponent.Instance.LoadOneConfig(typeof (T)) as T; // 当且仅当，服务端第一次配置为空时，程序域加载一次，其它任何时候不变
            }
        }
        void ISingleton.Register() {
            if (instance != null) {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            instance = (T)this;
        }
        void ISingleton.Destroy() {
            T t = instance;
            instance = null;
            t.Dispose();
        }
        bool ISingleton.IsDisposed() {
            throw new NotImplementedException();
        }
// ConfigSingleton: 桥接了这两个 ProtoObject 里的【反序列化】结束的接口        
        public override void AfterEndInit() { // 这里就是想要桥接：ProtoObject 里所实现过的【初始化前后】可以做的事情，接口，给框架使用者一些可用接口
        }
        public virtual void Dispose() {
        }
    }
}