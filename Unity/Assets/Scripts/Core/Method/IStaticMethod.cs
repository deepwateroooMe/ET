namespace ET
{
    // 抽象基类:  说， 不同的程序域里可能有不同参数类型的程序域里第一个静态调用方法，不如管理一下？
    public abstract class IStaticMethod
    {
        public abstract void Run();
        public abstract void Run(object a);
        public abstract void Run(object a, object b);
        public abstract void Run(object a, object b, object c);
    }
}