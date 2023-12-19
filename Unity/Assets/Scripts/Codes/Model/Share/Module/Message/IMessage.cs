namespace ET {
    public interface IMessage {}
    public interface IRequest: IMessage {
        int RpcId { get; set; }
    }
    public interface IResponse: IMessage {
        int Error { get; set; }
        string Message { get; set; }
        int RpcId { get; set; } // 身份标记：是，发向XX 的发送者 id, 还是接收消息的、接收者 id ？
    }
}
