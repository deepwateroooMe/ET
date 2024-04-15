namespace ET {
    public interface IMessage {} // 它没有什么返回类型

    public interface IRequest: IMessage {
        int RpcId {
            get;
            set;
        }
    }

    public interface IResponse: IMessage {
        int Error {
            get;
            set;
        }
        string Message {
            get;
            set;
        }
        int RpcId {
            get;
            set;
        }
    }
}